using RepositoryLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RepositoryLibrary.Extensions
{
    public static class DAL_Extensions
    {
        public static Dictionary<string, PropertyInfo> GetDatabaseAttributes<T, A>(this T instance) 
            where T : class 
            where A : DAL_AttributeBase 
        {
            Type instanceType = instance.GetType();
            Type attrType = typeof(A);
            var properties = instanceType.GetProperties();
            Dictionary<string, PropertyInfo> attrDictionary = new Dictionary<string, PropertyInfo>();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, attrType)) continue;
                var attr = prop.GetCustomAttribute(attrType) as A;
                string attrName = attr.Name;
                if (string.IsNullOrWhiteSpace(attrName))
                    throw new Exception($"ColumAttribute name not specified: {instanceType.AssemblyQualifiedName}");
                attrDictionary.Add(attrName, prop);
            }
            return attrDictionary;
        }
    }
}
