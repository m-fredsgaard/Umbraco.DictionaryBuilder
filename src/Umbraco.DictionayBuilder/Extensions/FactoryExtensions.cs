using Umbraco.Core.Composing;

namespace Umbraco.DictionaryBuilder.Extensions
{
    public static class FactoryExtensions
    {
        public static T GetInstance<T>(this IFactory factory) where T : class
        {
            T instance = factory.GetInstance(typeof(T)) as T;
            return instance;
        }
    }
}