using RepositoryLibrary.Interfaces;
using System;
using System.Data.SqlClient;

namespace RepositoryLibrary.OutputSources
{
    public class CommandOutputSource : IOutputSource
    {
        private SqlCommand _command;
        public CommandOutputSource(SqlCommand command)
        {
            _command = command;
        }
        public object this[string key]
        {
            get
            {
                return _command.Parameters[key].Value == null ? DBNull.Value : _command.Parameters[key].Value;
            }
        }
    }
}
