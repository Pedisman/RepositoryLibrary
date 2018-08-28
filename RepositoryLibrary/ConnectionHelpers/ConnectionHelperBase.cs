using System.Data.SqlClient;

namespace RepositoryLibrary.ConnectionHelpers
{
    public abstract class ConnectionHelperBase
    {
        private readonly string _connectionString;
        public ConnectionHelperBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open(); // open the connection
            return connection;
        }
    }
}
