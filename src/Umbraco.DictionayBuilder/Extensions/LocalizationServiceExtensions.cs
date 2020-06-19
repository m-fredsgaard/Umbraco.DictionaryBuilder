using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.DictionaryBuilder.Extensions
{
    public static class LocalizationServiceExtensions
    {
        public static string GetDictionaryValue(this ILocalizationService localizationService, string key, CultureInfo culture)
        {
            try
            {
                if(culture == null)
                    culture = CultureInfo.CurrentCulture;

                IDictionaryItem item = localizationService.GetDictionaryItemByKey(key);
                if (item == null)
                    return null;

                ILanguage umbracoLanguage = localizationService.GetLanguageByIsoCode(culture.Name)
                                            ?? localizationService.GetLanguageByIsoCode(culture
                                                .TwoLetterISOLanguageName);

                string languageValue = umbracoLanguage != null
                    ? item.Translations.SingleOrDefault(x => x.LanguageId == umbracoLanguage.Id)?.Value
                    : null;

                return !string.IsNullOrWhiteSpace(languageValue)
                    ? languageValue
                    : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}