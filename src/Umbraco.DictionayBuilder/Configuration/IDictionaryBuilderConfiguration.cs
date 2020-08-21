namespace Umbraco.DictionaryBuilder.Configuration
{
    public interface IDictionaryBuilderConfiguration
    {
        ModelsMode ModelsMode { get; }
        string DictionaryNamespace { get; }
        string DictionaryItemsPartialClassName { get; }
        string DictionaryDirectory { get; }
        bool AcceptUnsafeModelsDirectory { get; }
        bool UseNestedStructure { get; }
        bool GenerateFilePerDictionaryItem { get; }
        bool Enable { get; }
    }
}