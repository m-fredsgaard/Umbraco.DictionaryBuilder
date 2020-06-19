using System.Globalization;

namespace Umbraco.DictionaryBuilder.PluralizationRules
{
    public interface IPluralizationRule
    {
        CultureInfo[] Cultures { get; }

        int GetChoiceIndex(int choice, int choicesLength);
    }
}