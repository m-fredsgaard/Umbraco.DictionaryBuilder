using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Extensions;
using Umbraco.DictionaryBuilder.PluralizationRules;

namespace Umbraco.DictionaryBuilder
{
    public class DictionaryValue
    {
        internal const char DictionaryContainerPropertyName = '_';

        private readonly ILocalizationService _localizationService;
        private readonly Func<CultureInfo, string> _value;
        private object[] _args;

        private DictionaryValue(string key, Func<CultureInfo, string> value, params object[] args)
            : this(Current.Factory.GetInstance<ILocalizationService>(), key, value, args)
        { }

        private DictionaryValue(ILocalizationService localizationService, string key, Func<CultureInfo, string> value, params object[] args)
        {
            Key = key;
            _value = value;
            _args = args;
            _localizationService = localizationService;
        }

        public string Key { get; }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(string culture)
        {
            return ToString(CultureInfo.GetCultureInfo(culture));
        }

        public string ToString(CultureInfo culture)
        {
            string dictionaryValue = _localizationService.GetDictionaryValue(Key, culture) ?? _value(culture);
            if (dictionaryValue == null)
                return null;

            dictionaryValue = Pluralize(culture, dictionaryValue);

            return Regex.IsMatch(dictionaryValue, "(?!\\{)\\d+(?=\\})") && _args.Length > 0
                ? string.Format(dictionaryValue, _args ?? new object[0])
                : dictionaryValue;
        }

        public static DictionaryResolver Set()
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();
            
            return Set(GenerateKey(method), culture => null);
        }

        public static DictionaryResolver Set(string value)
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();
            
            return Set(GenerateKey(method), culture => value);
        }

        public static DictionaryResolver Set(Func<CultureInfo, string> value)
        {
            StackTrace stackTrace = new StackTrace();
            MethodBase method = stackTrace.GetFrame(1).GetMethod();

            return Set(GenerateKey(method), value);
        }

        private static DictionaryResolver Set(string key, Func<CultureInfo, string> value)
        {
            return args => new DictionaryValue(key, value, args);
        }

        private static string GenerateKey(MethodBase method)
        {
            string propertyName = method.MemberType == MemberTypes.Constructor
                ? DictionaryContainerPropertyName.ToString()
                : method.Name.Substring(method.Name.IndexOf(DictionaryContainerPropertyName) + 1)
                    .TrimEnd(DictionaryContainerPropertyName);

            return GenerateKey(method.DeclaringType, propertyName);
        }

        internal static string GenerateKey(Type type, string key)
        {
            if (!type.IsDictionaryContainer())
                return null;

            PropertyInfo dictionaryProperty = type.GetDictionaryProperty(key);
            if (dictionaryProperty == null)
                return null;

            DictionaryKeyAttribute dictionaryKey = dictionaryProperty.GetCustomAttribute<DictionaryKeyAttribute>();
            return GenerateKey(GenerateKey(type), dictionaryKey, dictionaryProperty.Name);
        }

        private static string GenerateKey(Type type)
        {
            if (!type.IsDictionaryContainer())
                return null;

            DictionaryKeyAttribute dictionaryKey = type.GetCustomAttribute<DictionaryKeyAttribute>();
            return GenerateKey(GenerateKey(type.DeclaringType), dictionaryKey,type.Name);
        }

        private static string GenerateKey(string parentKey, DictionaryKeyAttribute dictionaryKey, string key)
        {
            key = key == DictionaryContainerPropertyName.ToString() ? string.Empty : key;
            if (dictionaryKey == null)
                return GenerateKey(parentKey, key);

            key = dictionaryKey.Key;
            bool appendToParentKey = dictionaryKey.AppendToParentKey;

            return !appendToParentKey 
                ? key 
                : GenerateKey(parentKey, key);
        }

        private static string GenerateKey(string parentKey, string key)
        {
            if (parentKey.IsEmpty() && key.IsEmpty())
                return null;

            if (parentKey.IsEmpty())
                return key;

            if (key.IsEmpty())
                return parentKey;

            return $"{parentKey}.{key}";
        }

        private string Pluralize(CultureInfo culture, string dictionaryValue)
        {
            string[] pluralizedValues = dictionaryValue.Split('|').Select(x => x.Trim()).ToArray();
            if (pluralizedValues.Length <= 1) 
                return dictionaryValue;

            if (_args.Length > 0 && !(_args[0] is int))
                throw new ArgumentException(
                    "When the dictionary value contains a pluralization value (|'s), then the first argument has to be the count as int");

            int count = _args.Length == 0 ? 1 : (int) _args[0];
            _args = _args.Skip(1).ToArray();
            IPluralizationRule pluralizationRule =
                Current.Factory.GetAllInstances<IPluralizationRule>().SingleOrDefault(x => x.Cultures.Contains(culture)) 
                ?? new DefaultPluralizationRule();

            int index = pluralizationRule.GetChoiceIndex(count, pluralizedValues.Length);
            if (index >= pluralizedValues.Length)
                return dictionaryValue;

            return pluralizedValues[index]
                .Replace("{n}", count.ToString())
                .Replace("{count}", count.ToString());
        }

        public static implicit operator string(DictionaryValue dictionaryValue)
        {
            return dictionaryValue.ToString();
        }
    }
}