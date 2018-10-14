using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using RepositoryLibrary.Factories;

namespace RepositoryLibrary.Database
{
    public abstract class RepositoryBase
    {
        private RepositoryHelper _repositoryHelper;
        public RepositoryBase(string connectionString)
        {
            _repositoryHelper = new RepositoryHelper(connectionString,
                new ReaderOutputSourceFactory(),
                new CommandOutputSourceFactory());
        }

        #region Client Methods
        protected void AddInputParameters<T>(SqlCommand command, T instance, params string[] filter) where T : class
        {
            _repositoryHelper.AddParametersHelper(command, instance, ParameterDirection.Input, filter);
        }

        protected void AddOutputParameters<T>(SqlCommand command, T instance, params string[] filter) where T : class
        {
            _repositoryHelper.AddParametersHelper(command, instance, ParameterDirection.Output, filter);
        }

        protected void ExecuteNonQuery(SqlCommand command, string commandText)
        {
            _repositoryHelper.ExecuteNonQueryHelper(command, commandText, _repositoryHelper.SetupTextCommand);
        }

        protected void ExecuteNonQuerySP(SqlCommand command, string commandText)
        {
            _repositoryHelper.ExecuteNonQueryHelper(command, commandText, _repositoryHelper.SetupStoredProcedure);
        }

        protected T ExecuteScalar<T>(SqlCommand command, string commandText)
        {
            return _repositoryHelper.ExecuteScalarHelper<T>(command, commandText, _repositoryHelper.SetupTextCommand);
        }

        protected T ExecuteScalarSP<T>(SqlCommand command, string commandText)
        {
            return _repositoryHelper.ExecuteScalarHelper<T>(command, commandText, _repositoryHelper.SetupStoredProcedure);
        }

        protected T ExecuteNonQuery<T>(SqlCommand command, string commandText, params string[] filter) where T : class, new()
        {
            return _repositoryHelper.ExecuteNonQueryHelper<T>(command, commandText, _repositoryHelper.SetupTextCommand, filter);
        }

        protected T ExecuteNonQuerySP<T>(SqlCommand command, string commandText, params string[] filter) where T : class, new()
        {
            return _repositoryHelper.ExecuteNonQueryHelper<T>(command, commandText, _repositoryHelper.SetupStoredProcedure, filter);
        }

        protected IEnumerable<T> ExecuteReader<T>(SqlCommand command, string commandText, params string[] filter) where T : class, new()
        {
            return _repositoryHelper.ExecuteReaderHelper<T>(command, commandText, _repositoryHelper.SetupTextCommand, filter);
        }

        protected IEnumerable<T> ExecuteReaderSP<T>(SqlCommand command, string commandText, params string[] filter) where T : class, new()
        {
            return _repositoryHelper.ExecuteReaderHelper<T>(command, commandText, _repositoryHelper.SetupStoredProcedure, filter);
        }
        #endregion
    }
}
