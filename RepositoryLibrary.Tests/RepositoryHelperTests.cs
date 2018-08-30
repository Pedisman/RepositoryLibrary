using Moq;
using NUnit.Framework;
using RepositoryLibrary.Attributes;
using RepositoryLibrary.Database;
using RepositoryLibrary.Factories;
using RepositoryLibrary.Interfaces;
using RepositoryLibrary.Tests.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace RepositoryLibrary.Tests
{
    [TestFixture]
    public class RepositoryHelperTests
    {
        #region Test helpers
        private RepositoryHelper GetRepositoryHelper()
        {
            Mock<IOutputSourceFactory> readerOutputSourceFactory = new Mock<IOutputSourceFactory>();
            Mock<IOutputSourceFactory> commandOutputSourceFactory = new Mock<IOutputSourceFactory>();

            string connStr = "testConnStr";
            return new RepositoryHelper(connStr, readerOutputSourceFactory.Object, commandOutputSourceFactory.Object);
        }

        private SqlCommand GetOutputParameterCommand(TestClass testClass)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.Add(new SqlParameter("@name", System.Data.SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output, Value = testClass.Name });
            command.Parameters.Add(new SqlParameter("@age", System.Data.SqlDbType.Int) { Direction = ParameterDirection.Output, Value = testClass.Age });
            return command;
        }

        private IDataReader GetReader(IList<TestClass> objectsToEmulate)
        {
            var moq = new Mock<IDataReader>();

            // stores current reader position
            int index = -1;

            moq.Setup(x => x.Read())
                .Returns(() => index < objectsToEmulate.Count - 1)
                .Callback(() => index++);

            moq.Setup(x => x["NAME"])
                .Returns(() => objectsToEmulate[index].Name);

            moq.Setup(x => x["AGE"])
                .Returns(() => objectsToEmulate[index].Age);

            return moq.Object;
        }

        private TestClass GetSampleOutputParameterData()
        {
            return new TestClass
            {
                Name = "testName",
                Age = 15,
                NoAttributeProp = "Should be null"
            };
        }

        private List<TestClass> GetSampleReaderData()
        {
            return new List<TestClass>
            {
                new TestClass { Name = "Bob", Age = 64 },
                new TestClass { Name = "Henry", Age = 13 },
                new TestClass { Name = "Ashley", Age = 23 },
                new TestClass { Name = "Cass", Age = 31 },
            };            
        }
        #endregion       

        [TestCase]
        public void SetupCommandHelper_CommandTextAndConnectionAddedToCommand()
        {          
            SqlCommand command = new SqlCommand();
            SqlConnection connection = new SqlConnection();
            string commandText = "testCommandText";

            GetRepositoryHelper().SetupCommandHelper(command, connection, commandText);

            Assert.AreEqual(connection, command.Connection);
            Assert.AreEqual(commandText, command.CommandText);
        }

        [TestCase]
        public void SetupStoredProcedure_CommandTypeIsStoredProcedure()
        {
            SqlCommand command = new SqlCommand();
            SqlConnection connection = new SqlConnection();
            string commandText = "testCommandText";

            GetRepositoryHelper().SetupStoredProcedure(command, connection, commandText);

            Assert.AreEqual(CommandType.StoredProcedure, command.CommandType);
        }

        [TestCase]
        public void SetupStoredProcedure_CommandTypeIsText()
        {
            SqlCommand command = new SqlCommand();
            SqlConnection connection = new SqlConnection();
            string commandText = "testCommandText";

            GetRepositoryHelper().SetupTextCommand(command, connection, commandText);

            Assert.AreEqual(CommandType.Text, command.CommandType);
        }

        [TestCase]
        public void GetResultFromCommand_noFilter_returnsAllOutputProperties()
        {
            TestClass expected = GetSampleOutputParameterData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            SqlCommand command = GetOutputParameterCommand(expected);

            expected = new TestClass
            {
                Name = expected.Name,
                Age = expected.Age,
                NoAttributeProp = null
            };

            TestClass result = repositoryHelper.GetResultFromCommand<TestClass>(command, null);

            Assert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetResultFromCommand_filterName_returnsOnlyAge()
        {
            TestClass expected = GetSampleOutputParameterData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            SqlCommand command = GetOutputParameterCommand(expected);

            expected = new TestClass
            {
                Name = expected.Name,
                Age = 0,
                NoAttributeProp = null
            };

            TestClass result = repositoryHelper.GetResultFromCommand<TestClass>(command, new List<string> { "@name" });

            Assert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetResultFromCommand_filterAge_returnsOnlyAge()
        {
            TestClass expected = GetSampleOutputParameterData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            SqlCommand command = GetOutputParameterCommand(expected);

            expected = new TestClass
            {
                Name = null,
                Age = expected.Age,
                NoAttributeProp = null
            };

            TestClass result = repositoryHelper.GetResultFromCommand<TestClass>(command, new List<string> { "@age" });

            Assert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetResultFromReader_noFilter_returnsAllOutputProperties()
        {
            List<TestClass> expected = GetSampleReaderData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            IDataReader reader = GetReader(expected);
            expected.ForEach(x => x.NoAttributeProp = null); // set all objects age to 0

            var result = repositoryHelper.GetResultFromReader<TestClass>(reader, null);

            CollectionAssert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetResultFromReader_filterName_returnsSampleDataWithAgeSetTo0()
        {
            List<TestClass> expected = GetSampleReaderData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            IDataReader reader = GetReader(expected);

            expected.ForEach(x =>
            {
                x.Age = 0;
                x.NoAttributeProp = null;
            }); // set all objects age to 0

            var result = repositoryHelper.GetResultFromReader<TestClass>(reader, new List<string> { "NAME" });

            CollectionAssert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetResultFromReader_filterAge_returnsSampleDataWithNameSetToNull()
        {
            List<TestClass> expected = GetSampleReaderData();
            string connStr = "testConnStr";
            RepositoryHelper repositoryHelper = new RepositoryHelper(connStr, new ReaderOutputSourceFactory(), new CommandOutputSourceFactory());
            IDataReader reader = GetReader(expected);

            expected.ForEach(x =>
            {
                x.Name = null;
                x.NoAttributeProp = null;
            }); // set all objects age to 0

            var result = repositoryHelper.GetResultFromReader<TestClass>(reader, new List<string> { "AGE" });

            CollectionAssert.AreEqual(expected, result);
        }

        [TestCase]
        public void GetColumnAttributes_noFilter_attributesMatchColumnAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);            
            var dictionary = repositoryHelper.GetDatabaseAttributes<ColumnAttribute>(t, null);            
            PropertyInfo namePropertyInfo = t.GetProperty("Name");
            PropertyInfo agePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["NAME"]);
            Assert.AreEqual(agePropertyInfo, dictionary["AGE"]);
            Assert.AreEqual(2, dictionary.Count);            
        }

        [TestCase]
        public void GetColumnAttributes_filterNameAndAge_returnNameAndAgeColumnAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ColumnAttribute>(t, new List<string> { "NAME", "AGE" });
            PropertyInfo namePropertyInfo = t.GetProperty("Name");
            PropertyInfo agePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["NAME"]);
            Assert.AreEqual(agePropertyInfo, dictionary["AGE"]);
            Assert.AreEqual(2, dictionary.Count);
        }

        [TestCase]
        public void GetColumnAttributes_filterName_returnNameColumnAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ColumnAttribute>(t, new List<string> { "NAME" });
            PropertyInfo namePropertyInfo = t.GetProperty("Name");            
            Assert.AreEqual(namePropertyInfo, dictionary["NAME"]);            
            Assert.AreEqual(1, dictionary.Count);
        }

        [TestCase]
        public void GetColumnAttributes_filterAge_returnAgeColumnAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ColumnAttribute>(t, new List<string> { "AGE" });
            PropertyInfo namePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["AGE"]);
            Assert.AreEqual(1, dictionary.Count);
        }

        [TestCase]
        public void GetParameterAttributes_noFilter_returnsAllParameterAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ParameterAttribute>(t, null);
            PropertyInfo namePropertyInfo = t.GetProperty("Name");
            PropertyInfo agePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["@name"]);
            Assert.AreEqual(agePropertyInfo, dictionary["@age"]);
            Assert.AreEqual(2, dictionary.Count);
        }

        [TestCase]
        public void GetParameterAttributes_filterNameAndAge_returnsAllNameAndAgeParameterAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ParameterAttribute>(t, new List<string> { "@name", "@age" });
            PropertyInfo namePropertyInfo = t.GetProperty("Name");
            PropertyInfo agePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["@name"]);
            Assert.AreEqual(agePropertyInfo, dictionary["@age"]);
            Assert.AreEqual(2, dictionary.Count);
        }

        [TestCase]
        public void GetParameterAttributes_filterName_returnsNameParameterAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ParameterAttribute>(t, new List<string> { "@name" });
            PropertyInfo namePropertyInfo = t.GetProperty("Name");            
            Assert.AreEqual(namePropertyInfo, dictionary["@name"]);            
            Assert.AreEqual(1, dictionary.Count);
        }

        [TestCase]
        public void GetParameterAttributes_filterAge_returnsAgeParameterAttributes()
        {
            RepositoryHelper repositoryHelper = GetRepositoryHelper();
            Type t = typeof(TestClass);
            var dictionary = repositoryHelper.GetDatabaseAttributes<ParameterAttribute>(t, new List<string> { "@age" });
            PropertyInfo namePropertyInfo = t.GetProperty("Age");
            Assert.AreEqual(namePropertyInfo, dictionary["@age"]);
            Assert.AreEqual(1, dictionary.Count);
        }
    }
}
