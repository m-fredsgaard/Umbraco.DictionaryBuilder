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
            string dictionaryValue = model.ToString();
            
            return string.Format(dictionaryValue, args);
        }

        public static string Format(this DictionaryModel model, int count, params object[] args)
        {
            string dictionaryValue = model.ToString();

            string[] pluralizedValues = dictionaryValue.Split('|').Select(x => x.Trim()).ToArray();
            if (pluralizedValues.Length <= 1) 
                return dictionaryValue;

            IPluralizationRule pluralizationRule =
                Current.Factory.GetAllInstances<IPluralizationRule>().SingleOrDefault(x => x.Cultures.Contains(model.Culture))
                ?? new DefaultPluralizationRule();

            int index = pluralizationRule.GetChoiceIndex(count, pluralizedValues.Length);
            if (index >= pluralizedValues.Length)
                return dictionaryValue;

            dictionaryValue = pluralizedValues[index]
                .Replace("{n}", count.ToString())
                .Replace("{count}", count.ToString());

            return string.Format(dictionaryValue, args);
        }

        internal static bool IsValidGenerateCodeItemKey(this DictionaryModel model)
        {
            string[] itemKeyParts = model.ItemKey.Split('.');
            string[] parentItemKeyParts = model.ParentModel?.ItemKey.Split('.') ?? new string[0];

            bool equal = itemKeyParts.Length - parentItemKeyParts.Length == 1 && !string.IsNullOrWhiteSpace(itemKeyParts.Last());
            if (!equal)
                return false;

            for (int i = 0; i < parentItemKeyParts.Length; i++)
            {
                equal = parentItemKeyParts[i] == itemKeyParts[i];
                if(!equal)
                    break;
            }

            return equal;
        }

        internal static string GenerateCodeItemKey(this DictionaryModel model, bool configUseParentItemKeyPrefix)
        {
            if (!configUseParentItemKeyPrefix)
                return model.ItemKey;

            string[] itemKeyParts = model.ItemKey.Split('.');
            
            bool equal = IsValidGenerateCodeItemKey(model);
            
            return equal ? itemKeyParts.Last() : model.ItemKey;
        }
    }
}