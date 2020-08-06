using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.DictionaryBuilder.Services
{
    public class UmbracoService : IUmbracoService
    {
        private readonly ILocalizationService _localizationService;

        public UmbracoService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Get dictionary models
        /// </summary>
        /// <returns>An array of models</returns>
        public DictionaryModel[] GetDictionaryModels()
        {
            // Get all dictionaries from Umbraco database
            IDictionaryItem[] dictionaryItems = _localizationService.GetDictionaryItemDescendants(null).OrderBy(x => x.ItemKey).ToArray();
            
            // Generate models
            return GetDictionaryModels(dictionaryItems).OrderBy(x => x.ItemKey).ToArray();
        }

        /// <summary>
        /// Get dictionary models that are children to the parent model or if no parent model provided, then the root dictionary items
        /// </summary>
        /// <param name="dictionaryItems">The collection of all dictionary items</param>
        /// <param name="parentModel">The parent model</param>
        /// <returns></returns>
        private static IEnumerable<DictionaryModel> GetDictionaryModels(IDictionaryItem[] dictionaryItems, DictionaryModelWrapper parentModel = null)
        {
            if (dictionaryItems == null)
                yield break;

            // Iterate through dictionary items that are children to the parent model or if no parent model provided, then the root dictionary items
            foreach (IDictionaryItem dictionaryItem in dictionaryItems.Where(x => x.ParentId == parentModel?.Key))
            {
                // Create a model, return it and continue
                DictionaryModelWrapper model = new DictionaryModelWrapper(dictionaryItem.Key, dictionaryItem.ItemKey, parentModel);
                yield return model;

                // Generate child models and return them
                foreach (DictionaryModel childModel in GetDictionaryModels(dictionaryItems, model))
                    yield return childModel;
            }
        }
    }
}