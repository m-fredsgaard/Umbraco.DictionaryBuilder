namespace Umbraco.DictionaryBuilder.VueI18N.Configuration
{
    public interface IVueI18NConfiguration
    {
        string DictionaryHttpHandlerUrl { get; }
        int DictionaryClientCache { get; }
    }
}