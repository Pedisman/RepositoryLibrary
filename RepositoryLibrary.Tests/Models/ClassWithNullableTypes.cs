using RepositoryLibrary.Attributes;

namespace RepositoryLibrary.Tests.Models
{
    public class ClassWithNullableTypes
    {
        [Column(Name = "QUANTITY")]
        public int Quantity { get; set; }

        [Column(Name = "PRICE")]
        public double? Price { get; set; }
    }
}
