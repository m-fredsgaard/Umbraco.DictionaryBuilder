using System;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.DictionaryBuilder.Models
{
    internal class DictionaryModelWrapper : DictionaryModel
    {
        internal Guid Key { get; }

        internal DateTime LastModified { get; }

        public DictionaryModelWrapper(IDictionaryItem dictionaryItem, DictionaryModel parentModel = null)
            : this(dictionaryItem.Key, dictionaryItem.ItemKey, parentModel)
        {
            LastModified = new [] {dictionaryItem.UpdateDate, dictionaryItem.CreateDate, dictionaryItem.DeleteDate}.Max() ?? DateTime.Now;
        }

        internal DictionaryModelWrapper(Guid key, string itemKey, DictionaryModel parentModel = null)
            : base(itemKey, parentModel)
        {
            Key = key;
        }
    }
}