using System;
using Umbraco.DictionaryBuilder.Extensions;

namespace Umbraco.DictionaryBuilder
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]
    public class DictionaryKeyAttribute : Attribute
    {
        public string Key { get; }

        public bool AppendToParentKey { get; set; }

        public DictionaryKeyAttribute(string key)
        {
            if(key.IsEmpty()) throw new ArgumentNullException(nameof(key), "key parameter may not be empty.");
            if(key == DictionaryValue.DictionaryContainerPropertyName.ToString()) throw new ArgumentNullException(nameof(key), $"key parameter may not be {DictionaryValue.DictionaryContainerPropertyName}");
            
            Key = key;
        }
    }
}