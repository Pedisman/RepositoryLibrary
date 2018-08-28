using System;
using System.Collections.Generic;
using RepositoryLibrary.Extensions;
using System.Reflection;
using RepositoryLibrary.Attributes;
using RepositoryLibrary.OutputSources;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;
using RepositoryLibrary.Interfaces;

namespace RepositoryLibrary.Database
{
    public abstract class RepositoryBase
    {
        private string _connectionString;
        public RepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Connection Helpers
        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        #endregion

        #region Command Setup Helpers
        private void SetupCommandHelper(SqlConnection connection, SqlCommand command, string commandText)
        {
            command.CommandText = commandText;
            command.Connection = connection;
        }

        private void SetupStoredProcedure(SqlConnection connection, SqlCommand command, string commandText)
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;
            SetupCommandHelper(connection, command, commandText);
        }
        private void SetupTextCommand(SqlConnection connection, SqlCommand command, string commandText)
        {
            command.CommandType = System.Data.CommandType.Text;
            SetupCommandHelper(connection, command, commandText);
        }
        #endregion

        #region ExecuteHelpers
        private T ExecuteHelper<T>(IOutputSource src, Dictionary<string, PropertyInfo> attrDictionary) where T : class
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var entry in attrDictionary)
            {
                var type = entry.Value.PropertyType;
                entry.Value.SetValue(obj, Convert.ChangeType(src[entry.Key], type));
            }
            return obj;
        }

        private void ExecuteNonQueryHelper(SqlCommand command, string commandText, Action<SqlConnection, SqlCommand, string> commandSetup)
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(connection, command, commandText);
                command.ExecuteNonQuery();
            }
        }

        private T ExecuteNonQueryHelper<T>(SqlCommand command, string commandText, Action<SqlConnection, SqlCommand, string> commandSetup)
            where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(connection, command, commandText);
                command.ExecuteNonQuery();
                var attrDictionary = Activator.CreateInstance<T>().GetDatabaseAttributes<T, ParameterAttribute>();
                return ExecuteHelper<T>(new CommandOutputSource(command), attrDictionary);
            }
        }
        private IEnumerable<T> ExecuteReaderHelper<T>(SqlCommand command, string commandText, Action<SqlConnection, SqlCommand, string> commandSetup) where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(connection, command, commandText);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Collection<T> result = new Collection<T>();
                    while (reader.Read())
                    {
                        var attrDictionary = Activator.CreateInstance<T>().GetDatabaseAttributes<T, ColumnAttribute>();
                        result.Add(ExecuteHelper<T>(new ReaderOutputSource(reader), attrDictionary));
                    }
                    return result;
                }
            }
        }
        #endregion

        #region Parameter
        private SqlParameter CreateParameter(SqlCommand command, object value, ParameterAttribute attr, ParameterDirection direction)
        {
            SqlParameter param = new SqlParameter(attr.Name, attr.Type);            
            if (attr.Length != 0) param.Size = attr.Length;
            param.Direction = direction;
            param.Value = value;
            return param;
        }
        private void AddParametersHelper<T>(SqlCommand command, T instance, ParameterDirection direction) where T : class
        {
            Type instanceType = instance.GetType();
            Type attrType = typeof(ParameterAttribute);
            var properties = instanceType.GetProperties();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, attrType)) continue;
                var attr = prop.GetCustomAttribute(attrType) as ParameterAttribute;
                command.Parameters.Add(CreateParameter(command, prop.GetValue(instance), attr, direction));
            }
        }
        #endregion

        #region Client Methods
        protected void AddInputParameters<T>(SqlCommand command, T instance) where T : class
        {
            AddParametersHelper(command, instance, ParameterDirection.Input);
        }

        protected void AddOutputParameters<T>(SqlCommand command, T instance) where T : class
        {
            AddParametersHelper(command, instance, ParameterDirection.Output);
        }

        protected void ExecuteNonQuery(SqlCommand command, string commandText)
        {
            ExecuteNonQueryHelper(command, commandText, SetupTextCommand);
        }

        protected void ExecuteNonQuerySP(SqlCommand command, string commandText)
        {
            ExecuteNonQueryHelper(command, commandText, SetupStoredProcedure);
        }

        protected T ExecuteNonQuery<T>(SqlCommand command, string commandText) where T : class
        {
            return ExecuteNonQueryHelper<T>(command, commandText, SetupTextCommand);
        }

        protected T ExecuteNonQuerySP<T>(SqlCommand command, string commandText) where T : class
        {
            return ExecuteNonQueryHelper<T>(command, commandText, SetupStoredProcedure);
        }

        protected IEnumerable<T> ExecuteReader<T>(SqlCommand command, string commandText) where T : class
        {
            return ExecuteReaderHelper<T>(command, commandText, SetupTextCommand);
        }

        protected IEnumerable<T> ExecuteReaderSP<T>(SqlCommand command, string commandText) where T : class
        {
            return ExecuteReaderHelper<T>(command, commandText, SetupStoredProcedure);
        }
        #endregion
    }
}
