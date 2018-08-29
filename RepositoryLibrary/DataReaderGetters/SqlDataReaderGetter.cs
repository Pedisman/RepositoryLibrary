using System.Data;
using RepositoryLibrary.Interfaces;

namespace RepositoryLibrary.DataReaderGetters
{
    public class SqlDataReaderGetter : IDataReaderGetter
    {
        public IDataReader Get(IDbCommand command)
        {
            return command.ExecuteReader();
        }
    }
}
