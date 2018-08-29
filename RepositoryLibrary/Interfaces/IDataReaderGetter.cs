using System.Data;
namespace RepositoryLibrary.Interfaces
{
    public interface IDataReaderGetter
    {
        IDataReader Get(IDbCommand command);
    }
}
