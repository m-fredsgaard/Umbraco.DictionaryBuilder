using System;

namespace Umbraco.DictionaryBuilder.Models
{
    internal class DictionaryModelWrapper : DictionaryModel
    {
        internal Guid Key { get; }

        public DictionaryModelWrapper(Guid key, string itemKey, DictionaryModel parentModel = null)
            : base(itemKey, parentModel)
        {
            Key = key;
        }
    }
}