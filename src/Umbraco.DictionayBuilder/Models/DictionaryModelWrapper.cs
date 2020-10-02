using System;
using Umbraco.Core.Models;

namespace Umbraco.DictionaryBuilder.Models
{
    internal class DictionaryModelWrapper : DictionaryModel
    {
        internal Guid Key { get; }

        public DictionaryModelWrapper(IDictionaryItem dictionaryItem, DictionaryModel parentModel = null)
            : this(dictionaryItem.Key, dictionaryItem.ItemKey, parentModel)
        { }

        internal DictionaryModelWrapper(Guid key, string itemKey, DictionaryModel parentModel = null)
            : base(itemKey, parentModel)
        {
            Key = key;
        }
    }
}