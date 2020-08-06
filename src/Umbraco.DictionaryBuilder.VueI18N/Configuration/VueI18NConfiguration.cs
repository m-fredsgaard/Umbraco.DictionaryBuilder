using System.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N.Configuration
{
    public class VueI18NConfiguration : IVueI18NConfiguration
    {
        private const string Prefix = "Umbraco.DictionaryBuilder.";
        
        public string DictionaryHttpHandlerUrl { get; }

        public int DictionaryClientCache { get; }

        public VueI18NConfiguration()
        {
            var value = ConfigurationManager.AppSettings[Prefix + "HttpHandlerUrl"];
            if (!string.IsNullOrWhiteSpace(value))
                DictionaryHttpHandlerUrl = value;

            value = ConfigurationManager.AppSettings[Prefix + "DictionaryClientCache"];
            if (!int.TryParse(value, out int clientCache))
                DictionaryClientCache = clientCache;
        }
    }
}