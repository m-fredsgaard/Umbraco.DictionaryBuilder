using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Umbraco.DictionaryBuilder.Extensions
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string value)
        {
            Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            Regex whiteSpace = new Regex(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new Regex("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            // replace white spaces with underscore, then replace all invalid chars with empty string
            IEnumerable<string> pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(value, "_"), string.Empty)
                // split by underscores
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to upper case
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }

        public static string ToCamelCase(this string value)
        {
            Regex startsWithUpperCaseChar = new Regex("^[A-Z]");
            
            // get pascal case
            string pascalCase = value.ToPascalCase();
            // set first letter to lower case
            string camelCase = startsWithUpperCaseChar.Replace(pascalCase, m => m.Value.ToLower());
            return camelCase;
        }

        internal static string ToCodeString(this string value, bool isTypeName)
        {
            bool nextMustBeStartChar = true;
            if (value.Length == 0)
                return value;
            
            StringBuilder validTypeNameOrIdentifier = new StringBuilder();
            for (int i = 0; i < value.Length; ++i)
            {
                char ch = value[i];
                switch (char.GetUnicodeCategory(ch))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        nextMustBeStartChar = false;
                        break;
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        if (nextMustBeStartChar && ch != '_')
                            continue;
                        nextMustBeStartChar = false;
                        break;
                    default:
                        if (!isTypeName || !IsSpecialTypeChar(ch, ref nextMustBeStartChar))
                            continue;
                        break;
                }

                validTypeNameOrIdentifier.Append(ch);
            }
            return validTypeNameOrIdentifier.ToString();
        }

        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case '$':
                case '&':
                case '*':
                case '+':
                case ',':
                case '-':
                case '.':
                case ':':
                case '<':
                case '>':
                case '[':
                case ']':
                    nextMustBeStartChar = true;
                    return true;
                case '`':
                    return true;
                default:
                    return false;
            }
        }
    }
}