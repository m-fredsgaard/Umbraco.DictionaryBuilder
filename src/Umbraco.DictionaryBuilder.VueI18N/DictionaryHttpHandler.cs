using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
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
        private readonly object _lockObject = new object();
        private readonly IUmbracoService _umbracoService;
        private readonly IVueI18NConfiguration _configuration;

        public DictionaryHttpHandler(IUmbracoService umbracoService, IVueI18NConfiguration configuration)
        {
            _umbracoService = umbracoService;
            _configuration = configuration;
        }

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            string locale = context.Request.QueryString["l"] ?? context.Request.QueryString["locale"];
            string prefix = (context.Request.QueryString["p"] ?? context.Request.QueryString["prefix"]);

            CultureInfo culture = SetCurrentCulture(locale);

            string cacheKey = $"DictionaryItems_{culture.Name}_{prefix}";
            if (!(context.Cache[cacheKey] is string json))
            {
                try
                {
                    Monitor.Enter(_lockObject);
                    json = context.Cache[cacheKey] as string;
                    if (json == null)
                    {
                        string[] prefixes = prefix?.Split(',');
                        dynamic dictionaryItems = GetAllItems(prefixes);
                        if (dictionaryItems == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return;
                        }

                        JsonSerializerSettings jsonFormat = new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        };
                        json = JsonConvert.SerializeObject(dictionaryItems, jsonFormat);
                        int serverCache = _configuration.DictionaryServerCache;
                        if (serverCache > 0)
                        {
                            context.Cache.Add(cacheKey, json, null, DateTime.Now.AddMinutes(serverCache),
                                System.Web.Caching.Cache.NoSlidingExpiration,
                                CacheItemPriority.Default,
                                (key, value, reason) =>
                                    Log.Debug($"Cache key {key} is removed from cache with this reason: {reason}"));
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }

            // Set client cache
            int clientCache = _configuration.DictionaryClientCache;
            if (clientCache > 0)
            {
                context.Response.AddHeader("cache-control", "public");
                context.Response.AddHeader("cache-control", $"max-age={clientCache*60}");
            }

            string etag = $"{json.GetHashCode()}";
            if (context.Request.Headers["If-None-Match"] != null && context.Request.Headers["If-None-Match"].Equals(etag))
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            else
            {
                context.Response.AddHeader("etag", etag);
                
                context.Response.ContentType = "application/json";
                context.Response.Write(json); 

                if (context.Request.Headers["Accept-Encoding"].Contains("gzip"))
                {
                    // gzip
                    context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                    context.Response.AppendHeader("Content-Encoding", "gzip");
                }
                else if (context.Request.Headers["Accept-Encoding"].Contains("deflate") || context.Request.Headers["Accept-Encoding"] == "*")
                {
                    // deflate
                    context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                    context.Response.AppendHeader("Content-Encoding", "gzip");
                }
            }
        }

        private static CultureInfo SetCurrentCulture(string locale)
        {
            if(locale == null)
                return Thread.CurrentThread.CurrentCulture;

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
            return Thread.CurrentThread.CurrentCulture;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        /// <summary>
        /// Get all dictionary items for Vue app formatted as a tree in the shape that vue-I18n likes.
        /// </summary>
        /// <returns></returns>
        private dynamic GetAllItems(params string[] prefixes)
        {
            DictionaryModelWrapper[] dictionaryModels =
                _umbracoService.GetDictionaryModels(item =>
                    prefixes == null ||
                    !prefixes.Any() ||
                    prefixes.Any(prefix =>
                        item.ItemKey.Equals(prefix, StringComparison.InvariantCultureIgnoreCase) ||
                        item.ItemKey.StartsWith(prefix + ".", StringComparison.InvariantCultureIgnoreCase)))
                .OrderBy(item => item.GetItemKey())
                .Cast<DictionaryModelWrapper>()
                .ToArray();

            return GenerateVueI18N(dictionaryModels, null);
        }

        private dynamic GenerateVueI18N(DictionaryModelWrapper[] allDictionaryItems, DictionaryModelWrapper parent)
        {
            return GenerateVueI18N(allDictionaryItems, parent, out bool _);
        }

        private dynamic GenerateVueI18N(DictionaryModelWrapper[] allDictionaryItems, DictionaryModelWrapper parent, out bool hasChildItems)
        {
            Dictionary<dynamic, dynamic> items = new Dictionary<dynamic,dynamic>();

            DictionaryModelWrapper[] childDictionaryItems = allDictionaryItems.Where(x => x.GetParentModel() == parent).ToArray();
            hasChildItems = childDictionaryItems.Any();
            foreach (DictionaryModelWrapper dictionaryItem in childDictionaryItems)
            {
                string itemKey = dictionaryItem.GenerateCodeItemKey(_configuration.UseNestedStructure);
                
                itemKey = itemKey.ToCodeString(true).ToCamelCase();
                try
                {
                    items.Add($"${itemKey}", dictionaryItem.ToString());
                }
                catch(Exception e)
                {
                    throw new Exception($"Unable to add '${itemKey}' dictionaryItem", e);
                }

                dynamic childItems = GenerateVueI18N(allDictionaryItems, dictionaryItem, out bool addChildItems);
                if (!addChildItems) 
                    continue;

                try
                {
                    items.Add($"{itemKey}", childItems);
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to add '{itemKey}' dictionaryItem", e);
                }
            }

            return items;
        }

        private void AddDictionaryItem(string key, dynamic value)
        {
            items.Add($"{itemKey}", childItems);
        }
    }
}