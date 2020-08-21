using System;
using System.Globalization;
using System.Linq;
using Serilog;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.DictionaryBuilder.Factories
{
    public abstract class DictionaryFactory
    {
        private readonly ILocalizationService _localizationService;
        private static ILanguage[] _languages;

        
        // ReSharper disable once MemberCanBePrivate.Global
        // Internal to enable unit test
        internal ILanguage[] Languages
        {
            get
            {
                try
                {
                    return _languages ?? (_languages = _localizationService.GetAllLanguages().ToArray());
                }
                catch(Exception exception)
                {
                    Log.Error(exception, "Unable to get all languages from Umbraco database");
                    return new ILanguage[0];
                }
            }
        }

        protected DictionaryFactory()
            : this(Current.Services.LocalizationService)
        { }

        private DictionaryFactory(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public abstract void EnsureDictionaries();

        protected DictionaryModel Create(string itemKey)
        {
            return Create(itemKey, null, (string) null);
        }

        protected DictionaryModel Create(string itemKey, DictionaryModel parent)
        {
            return Create(itemKey, parent, (string) null);
        }

        protected DictionaryModel Create(string itemKey, string value)
        {
            return Create(itemKey, null, culture => value);
        }

        protected DictionaryModel Create(string itemKey, Func<CultureInfo, string> value)
        {
            return Create(itemKey, null, value);
        }

        protected DictionaryModel Create(string itemKey, DictionaryModel parent, string value)
        {
            return Create(itemKey, parent, culture => value);
        }

        protected DictionaryModel Create(string itemKey, DictionaryModel parent, Func<CultureInfo, string> valueResolver)
        {
            try
            {
                DictionaryModelWrapper parentModel = parent as DictionaryModelWrapper;

                // Get the dictionary item from Umbraco. Otherwise crete a new.
                IDictionaryItem dictionaryItem = _localizationService.GetDictionaryItemByKey(itemKey) ??
                                                 _localizationService.CreateDictionaryItemWithIdentity(itemKey,
                                                     parentModel?.Key);

                // Create the model
                DictionaryModelWrapper model = new DictionaryModelWrapper(dictionaryItem, parentModel);
                // If no value resolver are provided, then return the model
                if (valueResolver == null)
                    return model;

                // A save indicator
                bool save = false;
                // Iterate through all languages
                foreach (ILanguage language in Languages)
                {
                    IDictionaryTranslation translation =
                        dictionaryItem.Translations.SingleOrDefault(x => x.Language == language);
                    // If the dictionary translation already has a value, then don't change it
                    if (!string.IsNullOrEmpty(translation?.Value))
                        continue;

                    // Get the culture specific default value from the value resolver
                    string value = valueResolver(language.CultureInfo);
                    // If there is no value, then do nothing
                    if (value == null)
                        continue;

                    // Add the value to the Umbraco dictionary item
                    _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, language, value);
                    // Indicate that a value has been added to the dictionary, and ensure save
                    save = true;

                    // Log the addition
                    Log.Information(
                        $"Adding '{language}' value:'{value}' for dictionary item: '{dictionaryItem.ItemKey}'");
                }

                // Save the dictionary item
                if (save)
                    _localizationService.Save(dictionaryItem);

                // Return the model
                return model;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error trying to create a dictionary");
                return null;
            }
        }
    }
}