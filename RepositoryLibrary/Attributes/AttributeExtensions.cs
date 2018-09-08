using System;
using System.Linq.Expressions;

namespace RepositoryLibrary.Attributes
{
    public static class AttributeExtensions
    {
        #region Get property names
        private static string GetAttributeNameHelper<T, R, A>(Expression<Func<T, R>> selector) where A : DAL_AttributeBase
        {
            var mexpr = selector.Body as MemberExpression;
            if (mexpr == null) return null;
            if (mexpr.Member == null) return null;
            object[] attrs = mexpr.Member.GetCustomAttributes(typeof(A), false);
            if (attrs == null || attrs.Length == 0) return null;
            A desc = attrs[0] as A;
            if (desc == null) return null;
            return desc.Name;
        }

        public static string GetColumnName<T, R>(this T instance,
            Expression<Func<T, R>> selector) where T : class
        {
            return GetAttributeNameHelper<T, R, ColumnAttribute>(selector);
        }

        public static string GetParameterName<T, R>(this T instance,
            Expression<Func<T, R>> selector) where T : class
        {
            return GetAttributeNameHelper<T, R, ParameterAttribute>(selector);
        }
        #endregion
    }
}
