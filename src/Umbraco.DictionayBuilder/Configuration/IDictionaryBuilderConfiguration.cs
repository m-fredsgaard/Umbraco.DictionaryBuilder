namespace Umbraco.DictionaryBuilder.Configuration
{
    public interface IDictionaryBuilderConfiguration
    {
        ModelsMode ModelsMode { get; }
        string DictionaryNamespace { get; }
        string DictionaryItemsPartialClassName { get; }
        string DictionaryDirectory { get; }
        //string DictionaryHttpHandlerUrl { get; }
        //int DictionaryClientCache { get; }
        bool AcceptUnsafeModelsDirectory { get; }
        bool UseParentItemKeyPrefix { get; }
        bool Enable { get; }
    }
}