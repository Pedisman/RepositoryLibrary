using System;
using System.Data;
using RepositoryLibrary.Interfaces;

namespace RepositoryLibrary.IDbCommandExecutors
{
    public class SqlCommandExecutor : IDbCommandExecutor
    {
        public IDbCommand ExecuteNonQuery(IDbCommand command)
        {
            command.ExecuteNonQuery();
            return command;
        }
    }
}
