using System.Globalization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;

namespace Umbraco.DictionaryBuilder.Models
{
    public abstract class DictionaryModel
    {
        private ICultureDictionary _cultureDictionary;

        public string ItemKey { get; }

        internal DictionaryModel ParentModel { get; }
        
        internal CultureInfo Culture => CultureDictionary.Culture;

        private ICultureDictionary CultureDictionary => _cultureDictionary ?? (_cultureDictionary = Current.CultureDictionaryFactory.CreateDictionary());

        protected DictionaryModel(string itemKey)
            : this(itemKey, null)
        { }

        internal DictionaryModel(string itemKey, DictionaryModel parentModel)
        {
            ItemKey = itemKey;
            ParentModel = parentModel;
        }

        public override string ToString()
        {
            return CultureDictionary[ItemKey];
        }

        public static implicit operator string(DictionaryModel dictionaryModel)
        {
            return dictionaryModel.ToString();
        }
    }
}