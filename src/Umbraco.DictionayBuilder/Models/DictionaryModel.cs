using System;
using System.Globalization;
using Serilog;
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
            if(_resolver == null)
                _resolver = culture =>
                {
                    try
                    {
                        return Current.CultureDictionaryFactory.CreateDictionary();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, "Error trying to create an ICultureDictionary");
                        return null;
                    }
                };
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
            try
            {
                return _resolver?.Invoke(culture)?[_itemKey];
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Can't get the dictionary value");
                return null;
            }
        }

        public static implicit operator string(DictionaryModel dictionaryModel)
        {
            return dictionaryModel.ToString();
        }
    }
}