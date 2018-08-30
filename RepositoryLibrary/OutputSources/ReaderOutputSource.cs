using RepositoryLibrary.Interfaces;
using System;
using System.Data;

namespace RepositoryLibrary.OutputSources
{
    public class ReaderOutputSource : IOutputSource
    {
        private IDataReader _reader;
        public ReaderOutputSource(object inputReader)
        {
            IDataReader reader = inputReader as IDataReader;
            if (reader == null) throw new ArgumentException("inputReader");
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
