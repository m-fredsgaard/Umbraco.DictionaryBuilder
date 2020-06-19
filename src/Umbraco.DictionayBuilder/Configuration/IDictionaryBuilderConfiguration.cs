namespace Umbraco.DictionaryBuilder.Configuration
{
    public interface IDictionaryBuilderConfiguration
    {
        string DictionaryNamespace { get; }
        string DictionaryDirectory { get; }
        string DictionaryHttpHandlerUrl { get; }
        int DictionaryClientCache { get; }
        bool AcceptUnsafeModelsDirectory { get; }
        bool Enable { get; }
    }
}