using RepositoryLibrary.Attributes;

namespace RepositoryLibrary.Tests.Models
{
    public class AttributeTestClass
    {
        [Column(Name = "FIRST_NAME")]
        [Parameter(Name = "@firstName", Type = System.Data.SqlDbType.NVarChar, Length = 50)]
        public string FirstName { get; set; }

        [Column(Name = "LAST_NAME")]
        [Parameter(Name = "@lastName", Type = System.Data.SqlDbType.NVarChar, Length = 50)]
        public string LastName { get; set; }

        [Column(Name = "AGE")]
        [Parameter(Name = "@age", Type = System.Data.SqlDbType.Int)]
        public int Age { get; set; }
    }
}
