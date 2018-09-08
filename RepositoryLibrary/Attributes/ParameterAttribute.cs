using System.Data;

namespace RepositoryLibrary.Attributes
{
    public sealed class ParameterAttribute : DAL_AttributeBase
    {
        public SqlDbType Type { get; set; }
        public int Length { get; set; }
    }
}