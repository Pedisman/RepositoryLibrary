using RepositoryLibrary.Interfaces;
using System;
using System.Data.SqlClient;

namespace RepositoryLibrary.OutputSources
{
    public class ReaderOutputSource : IOutputSource
    {
        private SqlDataReader _reader;
        public ReaderOutputSource(SqlDataReader reader)
        {
            _reader = reader;
        }
        public object this[string key]
        {
            get
            {
                return _reader[key] == DBNull.Value ? null : _reader[key];
            }
        }
    }
}
