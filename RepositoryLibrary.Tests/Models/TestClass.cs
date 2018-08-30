using RepositoryLibrary.Attributes;

namespace RepositoryLibrary.Tests.Models
{
    public class TestClass
    {
        [Column(Name = "NAME")]
        [Parameter(Name = "@name", Type = System.Data.SqlDbType.VarChar, Length = 50)]
        public string Name { get; set; }

        [Column(Name = "AGE")]
        [Parameter(Name = "@age", Type = System.Data.SqlDbType.Int)]
        public int Age { get; set; }

        public string NoAttributeProp { get; set; }

        public override bool Equals(object obj)
        {
            TestClass testClass = obj as TestClass;
            if (testClass == null) return false;
            return Name == testClass.Name &&
                Age == testClass.Age &&
                NoAttributeProp == testClass.NoAttributeProp;
        }

        public override int GetHashCode()
        {
            int hash = 17;            
            hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
            hash = hash * 23 + Age.GetHashCode();
            hash = hash * 23 + (NoAttributeProp != null ? NoAttributeProp.GetHashCode() : 0);
            return hash;
        }
    }
}
