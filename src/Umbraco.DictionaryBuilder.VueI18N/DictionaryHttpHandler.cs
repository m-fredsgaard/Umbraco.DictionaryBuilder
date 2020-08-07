using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Umbraco.DictionaryBuilder.Extensions;
using Umbraco.DictionaryBuilder.Models;
using Umbraco.DictionaryBuilder.Services;
using Umbraco.DictionaryBuilder.VueI18N.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N
{
    public class DictionaryHttpHandler : IDictionaryHttpHandler
    {
        private readonly IUmbracoService _localizationService;
        private readonly IVueI18NConfiguration _configuration;

        public DictionaryHttpHandler(IUmbracoService localizationService, IVueI18NConfiguration configuration)
        {
            _localizationService = localizationService;
            _configuration = configuration;
        }

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            string locale = context.Request.QueryString["l"] ?? context.Request.QueryString["locale"];
            SetCurrentCulture(locale);

            string[] prefixes = (context.Request.QueryString["p"] ?? context.Request.QueryString["prefix"])?.Split(',');
            dynamic dictionaryItems = GetAllItems(out DateTime lastModified, prefixes);
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

        private static void SetCurrentCulture(string locale)
        {
            if(locale == null)
                return;

            try
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(locale);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException e)
            {
                Log.Warning(e, $"Culture {locale} not found. Using current culture: {CultureInfo.CurrentUICulture.Name}");
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        /// <summary>
        /// Get all dictionary items for Vue app formatted as a tree in the shape that vue-I18n likes.
        /// </summary>
        /// <returns></returns>
        private dynamic GetAllItems(out DateTime lastModified, params string[] prefixes)
        {
            DictionaryModelWrapper[] dictionaryModels = _localizationService.GetDictionaryModels(item => prefixes == null || !prefixes.Any() || prefixes.Any(prefix => item.ItemKey.Equals(prefix, StringComparison.InvariantCultureIgnoreCase) || item.ItemKey.StartsWith(prefix + ".", StringComparison.InvariantCultureIgnoreCase)))
                .OrderBy(item => item.ItemKey)
                .Cast<DictionaryModelWrapper>()
                .ToArray();

            lastModified = dictionaryModels.Select(x => x.LastModified).Max();

            return GenerateVueI18N(dictionaryModels, null);
        }

        private dynamic GenerateVueI18N(DictionaryModelWrapper[] allDictionaryItems, DictionaryModelWrapper parent)
        {
            return GenerateVueI18N(allDictionaryItems, parent, out bool _);
        }

        private dynamic GenerateVueI18N(DictionaryModelWrapper[] allDictionaryItems, DictionaryModelWrapper parent, out bool hasChildItems)
        {
            var items = new Dictionary<dynamic,dynamic>();

            DictionaryModelWrapper[] childDictionaryItems = allDictionaryItems.Where(x => x.ParentModel == parent).ToArray();
            hasChildItems = childDictionaryItems.Any();
            foreach (DictionaryModelWrapper dictionaryItem in childDictionaryItems)
            {
                string itemKey = dictionaryItem.GenerateCodeItemKey(_configuration.UseParentItemKeyPrefix);
                
                itemKey = itemKey.ToCodeString(true).ToCamelCase();
                items.Add($"${itemKey}", dictionaryItem.ToString());

                dynamic childItems = GenerateVueI18N(allDictionaryItems, dictionaryItem, out bool addChildItems);
                if (addChildItems)
                    items.Add($"{itemKey}", childItems);
            }

            return items;
        }
    }
}