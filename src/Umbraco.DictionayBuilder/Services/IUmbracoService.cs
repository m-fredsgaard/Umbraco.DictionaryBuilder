using System;
using Umbraco.Core.Models;
using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.DictionaryBuilder.Services
{
    public interface IUmbracoService
    {
        DictionaryModel[] GetDictionaryModels(Func<IDictionaryItem, bool> predicate = null);
    }
}