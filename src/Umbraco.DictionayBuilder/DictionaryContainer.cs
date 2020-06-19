namespace Umbraco.DictionaryBuilder
{
    public abstract class DictionaryContainer<T> where T : DictionaryContainer<T>, new()
    {
        private static DictionaryResolver _value;

        public static DictionaryResolver _
        {
            get
            {
                if (_value == null) 
                    new T();
                return _value;
            }
            protected set => _value = value;
        }
    }
}