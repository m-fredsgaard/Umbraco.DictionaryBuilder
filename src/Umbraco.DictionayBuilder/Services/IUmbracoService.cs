using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.DictionaryBuilder.Services
{
    public interface IUmbracoService
    {
        DictionaryModel[] GetDictionaryModels();
    }
}