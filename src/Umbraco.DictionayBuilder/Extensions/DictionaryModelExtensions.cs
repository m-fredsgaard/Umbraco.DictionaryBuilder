using System.Globalization;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.DictionaryBuilder.Models;
using Umbraco.DictionaryBuilder.PluralizationRules;

namespace Umbraco.DictionaryBuilder.Extensions
{
    public static class DictionaryModelExtensions
    {
        public static string Format(this DictionaryModel model, params object[] args)
        {
            return Format(model, CultureInfo.CurrentUICulture, args);
        }

        public static string Format(this DictionaryModel model, CultureInfo culture, params object[] args)
        {
            string dictionaryValue = model.ToString(culture);
            
            return string.Format(dictionaryValue, args);
        }

        public static string Format(this DictionaryModel model, int count, params object[] args)
        {
            return Format(model, CultureInfo.CurrentUICulture, count, args);
        }

        public static string Format(this DictionaryModel model, CultureInfo culture, int count, params object[] args)
        {
            string dictionaryValue = model.ToString(culture);

            string[] pluralizedValues = dictionaryValue.Split('|').Select(x => x.Trim()).ToArray();
            if (pluralizedValues.Length <= 1) 
                return dictionaryValue;

            IPluralizationRule pluralizationRule =
                Current.Factory.GetAllInstances<IPluralizationRule>().SingleOrDefault(x => x.Cultures.Contains(culture))
                ?? new DefaultPluralizationRule();

            int index = pluralizationRule.GetChoiceIndex(count, pluralizedValues.Length);
            if (index >= pluralizedValues.Length)
                return dictionaryValue;

            dictionaryValue = pluralizedValues[index]
                .Replace("{n}", count.ToString())
                .Replace("{count}", count.ToString());

            return string.Format(dictionaryValue, args);
        }

        internal static bool IsValidForNestedStructure(this DictionaryModel model)
        {
            DictionaryModel parentModel = model.GetParentModel();
            if (parentModel == null)
                return true;

            string[] itemKeyParts = model.GetItemKey().Split('.');
            string[] parentItemKeyParts = parentModel.GetItemKey().Split('.') ?? new string[0];

            bool equal = itemKeyParts.Length - parentItemKeyParts.Length == 1 && !string.IsNullOrWhiteSpace(itemKeyParts.Last());
            if (!equal)
                return false;

            if (!parentModel.IsValidForNestedStructure())
                return false;
            
            for (int i = 0; i < parentItemKeyParts.Length; i++)
            {
                equal = parentItemKeyParts[i] == itemKeyParts[i];
                if(!equal)
                    break;
            }

            return equal;
        }

        internal static bool RenderParentModel(this DictionaryModel model, bool useNestedStructure)
        {
            return useNestedStructure && model.IsValidForNestedStructure();
        }

        
        internal static bool IsRootModel(this DictionaryModel model, bool useNestedStructure)
        {
            return model.GetParentModel() == null || !useNestedStructure || !model.IsValidForNestedStructure();
        }

        internal static string GenerateCodeItemKey(this DictionaryModel model, bool useNestedStructure)
        {
            if (!useNestedStructure)
                return model.GetItemKey();

            string[] itemKeyParts = model.GetItemKey().Split('.');
            
            bool equal = IsValidForNestedStructure(model);
            
            return equal ? itemKeyParts.Last() : model.GetItemKey();
        }
    }
}