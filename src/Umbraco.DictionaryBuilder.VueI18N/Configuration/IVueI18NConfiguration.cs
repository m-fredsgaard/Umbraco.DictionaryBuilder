using Umbraco.DictionaryBuilder.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N.Configuration
{
    public interface IVueI18NConfiguration : IDictionaryBuilderConfiguration
    {
        string DictionaryHttpHandlerUrl { get; }
        int DictionaryServerCache { get; }
        int DictionaryClientCache { get; }
    }
}