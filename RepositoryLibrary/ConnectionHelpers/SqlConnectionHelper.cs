using System.Configuration;

namespace RepositoryLibrary.ConnectionHelpers
{
    public class SqlConnectionHelper : ConnectionHelperBase
    {
        private static string _sqlConnectionString = ConfigurationManager.ConnectionStrings["SqlConnStr"].ConnectionString;
        public SqlConnectionHelper() : base(_sqlConnectionString)
        {

        }
    }
}
