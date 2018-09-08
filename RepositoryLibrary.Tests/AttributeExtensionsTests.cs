using NUnit.Framework;
using RepositoryLibrary.Attributes;
using RepositoryLibrary.Tests.Models;

namespace RepositoryLibrary.Tests
{
    [TestFixture]
    public class AttributeExtensionsTests
    {
        [TestCase]
        public void GetColumnName_returnsColumnName()
        {
            AttributeTestClass testClass = new AttributeTestClass();
            string result = testClass.GetColumnName(x => x.Age);
            Assert.AreEqual("AGE", result);
        }

        [TestCase]
        public void GetParameterName_returnsParameterName()
        {
            AttributeTestClass testClass = new AttributeTestClass();
            string result = testClass.GetParameterName(x => x.Age);            
            Assert.AreEqual("@age", result);
        }
    }
}
