using RepositoryLibrary.Interfaces;
using System;
using System.Data.SqlClient;

namespace RepositoryLibrary.OutputSources
{
    public class ReaderOutputSource : IOutputSource
    {
        private SqlDataReader _reader;
        public ReaderOutputSource(object sqlReader)
        {
            SqlDataReader reader = sqlReader as SqlDataReader;
            if (reader == null) throw new ArgumentException("sqlReader");
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
