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
        // Todo: Rename to something like: ...Nested...
        bool UseNestedStructure { get; }
        bool GenerateFilePerDictionaryItem { get; }
        bool Enable { get; }
    }
}