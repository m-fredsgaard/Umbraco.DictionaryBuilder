using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.VueI18N.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N
{
    public interface IDictionaryHttpHandler : IHttpHandler, IRouteHandler
    {}
    public class DictionaryHttpHandler : IDictionaryHttpHandler
    {
        private readonly ILocalizationService _localizationService;
        private readonly IVueI18NConfiguration _configuration;

        public DictionaryHttpHandler(ILocalizationService localizationService, IVueI18NConfiguration configuration)
        {
            _localizationService = localizationService;
            _configuration = configuration;
        }

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            string locale = context.Request.QueryString["l"] ?? context.Request.QueryString["locale"];

            string[] prefixes = (context.Request.QueryString["p"] ?? context.Request.QueryString["prefix"])?.Split(',');
            DateTime lastModified = default;
            dynamic dictionaryItems = GetAllItems(ref lastModified, locale, prefixes);
            if (dictionaryItems == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }

            if(lastModified == default)
                lastModified = DateTime.UtcNow;
            
            JsonSerializerSettings jsonFormat = new JsonSerializerSettings 
            { 
                ContractResolver = new CamelCasePropertyNamesContractResolver() 
            };
            string json = JsonConvert.SerializeObject(dictionaryItems, jsonFormat);
            
            // Set client cache
            int clientCache = _configuration.DictionaryClientCache;
            if (clientCache > 0)
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(clientCache));
                context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(clientCache));
                context.Response.AddHeader("Last-Modified", lastModified.ToLongDateString());
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(json); 
        }
        
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        /// <summary>
        /// Get all dictionary items for Vue app formatted as a tree in the shape that vue-I18n likes.
        /// </summary>
        /// <returns></returns>
        private dynamic GetAllItems(ref DateTime lastModified, string locale, params string[] prefixes)
        {
            IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages();

            List<IDictionaryItem> dictionaryItems = _localizationService.GetDictionaryItemDescendants(null)
                .OrderBy(item => item.ItemKey)
                .Where(item => prefixes == null || !prefixes.Any() || prefixes.Any(prefix => item.ItemKey.Equals(prefix, StringComparison.InvariantCultureIgnoreCase) || item.ItemKey.StartsWith(prefix + ".", StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            if (locale != null)
            {
                ILanguage language = languages.SingleOrDefault(x => x.CultureInfo.Name.Equals(locale, StringComparison.InvariantCultureIgnoreCase));
                return language == null 
                    ? null 
                    : GenerateVueI18N(dictionaryItems, null, 0, language, ref lastModified);
            }
            var items = new Dictionary<string,dynamic>();
            foreach (ILanguage language in languages)
            {
                string key = language.CultureInfo.Name;
                dynamic value = GenerateVueI18N(dictionaryItems, null, 0, language, ref lastModified);
                items.Add(key, value);
            }
            return items;
        }

        private static dynamic GenerateVueI18N(IEnumerable<IDictionaryItem> items, string parentKey, int level, ILanguage language, ref DateTime lastModified)
        {
            // Group items by the level we're at
            ILookup<string, IDictionaryItem> itemsByLevel = items.ToLookup(x => x.ItemKey.Split('.')[level]);

            // Format items as tree
            var itemTree = new Dictionary<dynamic, dynamic>();
            foreach (var t1 in itemsByLevel
                .Select(item => new {item, itemsToRecurse = item.Where(x => x.ItemKey.Count(c => c == '.') > level)}))
            {
                dynamic value;
                string key = string.IsNullOrEmpty(parentKey)
                    ? t1.item.Key
                    : parentKey + "." + t1.item.Key;
                if (t1.itemsToRecurse.Any())
                {
                    // Recurse to next level
                    value = GenerateVueI18N(t1.itemsToRecurse, key, level + 1, language, ref lastModified);
                }
                else
                {
                    // At leaf; emit translation
                    IDictionaryTranslation translation = t1.item.First().Translations.FirstOrDefault(t => t.LanguageId == language.Id);
                    if (translation != null)
                    {
                        lastModified = new [] {lastModified, translation.CreateDate, translation.UpdateDate}.Max();
                    }
                    value = translation?.Value;
                }

                itemTree.Add(t1.item.Key, value);
            }
            return itemTree;
        }
    }
}