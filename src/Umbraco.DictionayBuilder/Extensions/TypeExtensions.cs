using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;

namespace Umbraco.DictionaryBuilder.Extensions
{
    public static class TypeExtensions
    {
        internal static bool IsDictionaryContainer(this Type type)
        {
            if (type == null)
                return false;

            if (type.IsAbstract)
                return false;

            if (type.BaseType == null)
                return false;

            if (!type.BaseType.IsGenericType)
                return false;

            Type dictionaryContainerType = typeof(DictionaryContainer<>);

            Type[] genericArguments = null;
            Type baseType = type.BaseType;
            while (baseType != null)
            {
                if(baseType == typeof(object))
                    return false;

                if(!baseType.IsGenericType)
                    return false;

                if (dictionaryContainerType == baseType.GetGenericTypeDefinition())
                {
                    genericArguments = baseType.GetGenericArguments();
                    break;
                }

                baseType = baseType.BaseType;
            } 

            if (genericArguments == null)
                return false;

            if (genericArguments[0] == type && type.IsSubclassOf(dictionaryContainerType.MakeGenericType(type))) 
                return true;

            Log.Warning($"{type.FullName} doesn't inherits from 'DictionaryContainer<{genericArguments[0]}>', and is therefore ignored as dictionary container");
            return false;
        }

        internal static PropertyInfo GetDictionaryProperty(this Type type, string propertyName)
        {
            return GetDictionaryProperties(type).SingleOrDefault(x => x.Name == propertyName);
        }

        internal static IEnumerable<PropertyInfo> GetDictionaryProperties(this Type type)
        {
            if(!type.IsDictionaryContainer())
                return new PropertyInfo[0];

            return new []{type, type.BaseType}.SelectMany(x => x.GetProperties(BindingFlags.Static | BindingFlags.Public))
                .Where(x => x.PropertyType == typeof(DictionaryResolver));
        }
    }
}