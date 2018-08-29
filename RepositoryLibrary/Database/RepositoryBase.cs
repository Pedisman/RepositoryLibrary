using System;
using System.Collections.Generic;
using System.Reflection;
using RepositoryLibrary.Attributes;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data;
using RepositoryLibrary.Interfaces;
using System.Linq;
using RepositoryLibrary.Factories;
using RepositoryLibrary.DataReaderGetters;
using RepositoryLibrary.IDbCommandExecutors;

namespace RepositoryLibrary.Database
{
    public abstract class RepositoryBase
    {
        private string _connectionString;
        private IOutputSourceFactory _readerOutputSourceFactory;
        private IOutputSourceFactory _commandOutputSourceFactory;
        private IDataReaderGetter _dataReaderGetter;
        private IDbCommandExecutor _dbCommandExecutor;

        public RepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
            _readerOutputSourceFactory = new ReaderOutputSourceFactory();
            _commandOutputSourceFactory = new CommandOutputSourceFactory();
            _dataReaderGetter = new SqlDataReaderGetter();
            _dbCommandExecutor = new SqlCommandExecutor();
        }

        public RepositoryBase(string connectionString, 
            IOutputSourceFactory readerOutputSourceFactory,
            IOutputSourceFactory commandOutputSourceFactory,
            IDataReaderGetter dataReaderGetter,
            IDbCommandExecutor dbCommandExecutor)
        {
            _connectionString = connectionString;
            _readerOutputSourceFactory = readerOutputSourceFactory;
            _commandOutputSourceFactory = commandOutputSourceFactory;
            _dataReaderGetter = dataReaderGetter;
            _dbCommandExecutor = dbCommandExecutor;
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
        public static Dictionary<string, PropertyInfo> GetDatabaseAttributes<A>(Type instanceType, IEnumerable<string> filter)            
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

        private T ExecuteNonQueryHelper<T>(SqlCommand command, string commandText, Action<SqlConnection, SqlCommand, string> commandSetup, IEnumerable<string> filter)
            where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(connection, command, commandText);
                command = _dbCommandExecutor.ExecuteNonQuery(command) as SqlCommand;
                var attrDictionary = GetDatabaseAttributes<ParameterAttribute>(typeof(T), filter);
                return ExecuteHelper<T>(_commandOutputSourceFactory.Create(command), attrDictionary);
            }
        }
        private IEnumerable<T> ExecuteReaderHelper<T>(SqlCommand command, string commandText, Action<SqlConnection, SqlCommand, string> commandSetup, IEnumerable<string> filter) where T : class
        {
            using (SqlConnection connection = GetConnection())
            {
                commandSetup(connection, command, commandText);
                using (IDataReader reader = _dataReaderGetter.Get(command))
                {
                    Collection<T> result = new Collection<T>();
                    while (reader.Read())
                    {
                        var attrDictionary = GetDatabaseAttributes<ColumnAttribute>(typeof(T), filter);
                        result.Add(ExecuteHelper<T>(_readerOutputSourceFactory.Create(reader), attrDictionary));
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
        private void AddParametersHelper<T>(SqlCommand command, T instance, ParameterDirection direction, IEnumerable<string> filter) where T : class
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

        #region Client Methods
        protected void AddInputParameters<T>(SqlCommand command, T instance, IEnumerable<string> filter = null) where T : class
        {
            AddParametersHelper(command, instance, ParameterDirection.Input, filter);
        }

        protected void AddOutputParameters<T>(SqlCommand command, T instance, IEnumerable<string> filter = null) where T : class
        {
            AddParametersHelper(command, instance, ParameterDirection.Output, filter);
        }

        protected void ExecuteNonQuery(SqlCommand command, string commandText)
        {
            ExecuteNonQueryHelper(command, commandText, SetupTextCommand);
        }

        protected void ExecuteNonQuerySP(SqlCommand command, string commandText)
        {
            ExecuteNonQueryHelper(command, commandText, SetupStoredProcedure);
        }

        protected T ExecuteNonQuery<T>(SqlCommand command, string commandText, IEnumerable<string> filter = null) where T : class
        {
            return ExecuteNonQueryHelper<T>(command, commandText, SetupTextCommand, filter);
        }

        protected T ExecuteNonQuerySP<T>(SqlCommand command, string commandText, IEnumerable<string> filter = null) where T : class
        {
            return ExecuteNonQueryHelper<T>(command, commandText, SetupStoredProcedure, filter);
        }

        protected IEnumerable<T> ExecuteReader<T>(SqlCommand command, string commandText, IEnumerable<string> filter = null) where T : class
        {
            return ExecuteReaderHelper<T>(command, commandText, SetupTextCommand, filter);
        }

        protected IEnumerable<T> ExecuteReaderSP<T>(SqlCommand command, string commandText, IEnumerable<string> filter = null) where T : class
        {
            return ExecuteReaderHelper<T>(command, commandText, SetupStoredProcedure, filter);
        }
        #endregion
    }
}
