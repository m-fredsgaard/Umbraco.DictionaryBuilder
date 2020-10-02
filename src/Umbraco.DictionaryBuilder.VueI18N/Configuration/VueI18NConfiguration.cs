using System.Configuration;
using Umbraco.DictionaryBuilder.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N.Configuration
{
    public class VueI18NConfiguration : DictionaryBuilderConfiguration, IVueI18NConfiguration
    {
        private const string Prefix = "Umbraco.DictionaryBuilder.";
        private const string DefaultDictionaryHttpHandlerUrl = "umbraco/dictionaries.json";
        private const int DefaultDictionaryServerCache = 60;
        private const int DefaultDictionaryClientCache = 60;
        
        public string DictionaryHttpHandlerUrl { get; }
        public int DictionaryServerCache { get; }

        public int DictionaryClientCache { get; }

        public VueI18NConfiguration()
        {
            // ensure defaults are initialized for tests
            DictionaryHttpHandlerUrl = DefaultDictionaryHttpHandlerUrl;
            DictionaryServerCache = DefaultDictionaryServerCache;
            DictionaryClientCache = DefaultDictionaryClientCache;

            string value = ConfigurationManager.AppSettings[Prefix + "HttpHandlerUrl"];
            if (!string.IsNullOrWhiteSpace(value))
                DictionaryHttpHandlerUrl = value;

            value = ConfigurationManager.AppSettings[Prefix + "DictionaryServerCache"];
            if (int.TryParse(value, out int serverCache))
                DictionaryServerCache = serverCache;

            value = ConfigurationManager.AppSettings[Prefix + "DictionaryClientCache"];
            if (int.TryParse(value, out int clientCache))
                DictionaryClientCache = clientCache;
        }
    }
}