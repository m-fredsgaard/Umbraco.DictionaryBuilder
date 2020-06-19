namespace Umbraco.DictionaryBuilder.Configuration
{
    public interface IConfiguration
    {
        string DictionaryHttpHandlerUrl { get; }
        int DictionaryClientCache { get; }
    }
}