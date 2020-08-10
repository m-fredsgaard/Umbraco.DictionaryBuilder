using System.Globalization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;

namespace Umbraco.DictionaryBuilder.Models
{
    public delegate ICultureDictionary DictionaryResolver(CultureInfo culture);

    public abstract class DictionaryModel
    {
        private static DictionaryResolver _resolver;
        private readonly string _itemKey;
        private readonly DictionaryModel _parentModel;

        protected DictionaryModel(string itemKey)
            : this(itemKey, null)
        { }

        internal DictionaryModel(string itemKey, DictionaryModel parentModel)
        {
            _itemKey = itemKey;
            _parentModel = parentModel;
            _resolver = culture => Current.CultureDictionaryFactory.CreateDictionary();
        }

        public string GetItemKey()
        {
            return _itemKey;
        }

        internal DictionaryModel GetParentModel()
        {
            return _parentModel;
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentUICulture);
        }

        public string ToString(CultureInfo culture)
        {
            return _resolver(culture)[_itemKey];
        }

        public static implicit operator string(DictionaryModel dictionaryModel)
        {
            return dictionaryModel.ToString();
        }
    }
}