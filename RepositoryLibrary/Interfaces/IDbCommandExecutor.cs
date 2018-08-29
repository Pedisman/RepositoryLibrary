using System.Data;

namespace RepositoryLibrary.Interfaces
{
    public interface IDbCommandExecutor
    {
        IDbCommand ExecuteNonQuery(IDbCommand command);
    }
}
