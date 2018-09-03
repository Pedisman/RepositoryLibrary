using RepositoryLibrary.Attributes;
using RepositoryLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace RepositoryLibrary.Database
{
    public class RepositoryHelper
    {
        private string _connectionString;
        private IOutputSourceFactory _readerOutputSourceFactory;
        private IOutputSourceFactory _commandOutputSourceFactory;

        internal RepositoryHelper(string connectionString,
            IOutputSourceFactory readerOutputSourceFactory,
            IOutputSourceFactory commandOutputSourceFactory)
        {
            _connectionString = connectionString;
            _readerOutputSourceFactory = readerOutputSourceFactory;
            _commandOutputSourceFactory = commandOutputSourceFactory;
        }

        #region Connection Helpers
        internal SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        #endregion

        #region Conversion Helper
        object ChangeType(object value, Type conversion)
        {
            var t = conversion;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            return Convert.ChangeType(value, t);
        }
        #endregion

        #region Command Setup Helpers
        internal void SetupCommandHelper(SqlCommand command, SqlConnection connection, string commandText)
        {
            command.CommandText = commandText;
            command.Connection = connection;
        }

        internal void SetupStoredProcedure(SqlCommand command, SqlConnection connection, string commandText)
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;
            SetupCommandHelper(command, connection, commandText);
        }
        internal void SetupTextCommand(SqlCommand command, SqlConnection connection, string commandText)
        {
            command.CommandType = System.Data.CommandType.Text;
            SetupCommandHelper(command, connection, commandText);
        }
        #endregion

        #region Data extraction helpers
        internal T GetResultFromCommand<T>(IDbCommand command, IEnumerable<string> filter) where T : class
        {
            var attrDictionary = GetDatabaseAttributes<ParameterAttribute>(typeof(T), filter);
            return ExecuteHelper<T>(_commandOutputSourceFactory.Create(command), attrDictionary);
        }

        internal IEnumerable<T> GetResultFromReader<T>(IDataReader reader, IEnumerable<string> filter) where T : class
        {
            Collection<T> result = new Collection<T>();
            while (reader.Read())
            {
                var attrDictionary = GetDatabaseAttributes<ColumnAttribute>(typeof(T), filter);
                result.Add(ExecuteHelper<T>(_readerOutputSourceFactory.Create(reader), attrDictionary));
            }
            return result;
        }
        #endregion

        #region ExecuteHelpers
        internal Dictionary<string, PropertyInfo> GetDatabaseAttributes<A>(Type instanceType, IEnumerable<string> filter)
            where A : DAL_AttributeBase
        {
            Type attrType = typeof(A);
            var properties = instanceType.GetProperties();
            Dictionary<string, PropertyInfo> attrDictionary = new Dictionary<string, PropertyInfo>();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, attrType)) continue;
                var attr = prop.GetCustomAttribute(attrType) as A;
                if (filter != null && !filter.Contains(attr.Name)) continue;
                string attrName = attr.Name;
                if (string.IsNullOrWhiteSpace(attrName))
                    throw new Exception($"ColumAttribute name not specified: {instanceType.AssemblyQualifiedName}");
                attrDictionary.Add(attrName, prop);
            }
            return attrDictionary;
        }

        internal T ExecuteHelper<T>(IOutputSource src, Dictionary<string, PropertyInfo> attrDictionary) where T : class
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var entry in attrDictionary)
            {
                var type = entry.Value.PropertyType;
                entry.Value.SetValue(obj, ChangeType(src[entry.Key], type));
            }
            return obj;
        }

        internal T ExecuteScalarHelper<T>(SqlCommand command, string commandText, Action<SqlCommand, SqlConnection, string> commandSetup)
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(command, connection, commandText);
                return (T)ChangeType(command.ExecuteScalar(), typeof(T));                           
            }
        }

        internal void ExecuteNonQueryHelper(SqlCommand command, string commandText, Action<SqlCommand, SqlConnection, string> commandSetup)
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(command, connection, commandText);
                command.ExecuteNonQuery();
            }
        }

        internal T ExecuteNonQueryHelper<T>(SqlCommand command, string commandText, Action<SqlCommand, SqlConnection, string> commandSetup, IEnumerable<string> filter)
            where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(command, connection, commandText);
                command.ExecuteNonQuery();
                return GetResultFromCommand<T>(command, filter);
            }
        }

        internal IEnumerable<T> ExecuteReaderHelper<T>(SqlCommand command, string commandText, Action<SqlCommand, SqlConnection, string> commandSetup, IEnumerable<string> filter) where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(command, connection, commandText);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    return GetResultFromReader<T>(reader, filter);
                }
            }
        }
        #endregion

        #region Parameter
        internal SqlParameter CreateParameter(SqlCommand command, object value, ParameterAttribute attr, ParameterDirection direction)
        {
            SqlParameter param = new SqlParameter(attr.Name, attr.Type);
            if (attr.Length != 0) param.Size = attr.Length;
            param.Direction = direction;
            param.Value = value;
            return param;
        }
        internal void AddParametersHelper<T>(SqlCommand command, T instance, ParameterDirection direction, IEnumerable<string> filter) where T : class
        {
            Type instanceType = instance.GetType();
            Type attrType = typeof(ParameterAttribute);
            var properties = instanceType.GetProperties();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, attrType)) continue;
                var attr = prop.GetCustomAttribute(attrType) as ParameterAttribute;
                if (filter == null || filter.Contains(attr.Name))
                    command.Parameters.Add(CreateParameter(command, prop.GetValue(instance), attr, direction));
            }
        }
        #endregion
    }
}
