using System;
using System.Globalization;

namespace Umbraco.DictionaryBuilder.PluralizationRules
{
    public class DefaultPluralizationRule : IPluralizationRule
    {
        public CultureInfo[] Cultures => new []{ CultureInfo.InvariantCulture };

        // Inspired by: https://github.com/kazupon/vue-i18n/blob/8c0b4ff254140406f652cedf5403e794e8a2ea47/src/index.js#L562
        public int GetChoiceIndex(int choice, int choicesLength)
        {
            int index = Math.Abs(choice);

            if (choicesLength == 2)
            {
                return index > 1
                    ? 1
                    : 0;
            }

            return Math.Min(index, 2);
        }
    }
}