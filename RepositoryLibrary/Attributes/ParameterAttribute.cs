using System.Data;

namespace RepositoryLibrary.Attributes
{
    public class ParameterAttribute : DAL_AttributeBase
    {
        public SqlDbType Type { get; set; }
        public int Length { get; set; }
    }
}