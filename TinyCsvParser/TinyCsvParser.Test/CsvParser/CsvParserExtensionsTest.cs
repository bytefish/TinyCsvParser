// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TinyCsvParser.Mapping;

namespace TinyCsvParser.Test.CsvParser
{
    [TestFixture]
    public class CsvParserExtensionsTest
    {
        private class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime BirthDate { get; set; }
        }

        private class CsvPersonMapping : CsvMapping<Person>, ICsvMapping<Person>
        {
            public CsvPersonMapping()
            {
                MapProperty(0, x => x.FirstName);
                MapProperty(1, x => x.LastName);
                MapProperty(2, x => x.BirthDate);
            }
        }


        [Test]
        public void ReadFromFile_null_Test()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';', 1, true);
            ICsvMapping<Person> csvMapper = new CsvPersonMapping();
            CsvParser<Person> csvParser = new CsvParser<Person>(csvParserOptions, csvMapper);

            Assert.Throws<ArgumentNullException>(() => csvParser.ReadFromFile(null, Encoding.UTF8));
        }

        [Test]
        public void ReadFromFileTest()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';', 1, true);
            ICsvMapping<Person> csvMapper = new CsvPersonMapping();
            CsvParser<Person> csvParser = new CsvParser<Person>(csvParserOptions, csvMapper);

            var stringBuilder = new StringBuilder()
                .AppendLine("FirstName;LastName;BirthDate")
                .AppendLine("     Philipp;Wagner;1986/05/12       ")
                .AppendLine("Max;Mustermann;2014/01/01");
#if NETCOREAPP1_1
            var basePath = AppContext.BaseDirectory;
#else 
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
#endif
            var filePath = Path.Combine(basePath, "test_file.txt");

            File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);

            var result = csvParser
                .ReadFromFile(filePath.ToString(), Encoding.UTF8)
                .ToList();

            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(result.All(x => x.IsValid));

            Assert.AreEqual("Philipp", result[0].Result.FirstName);
            Assert.AreEqual("Wagner", result[0].Result.LastName);

            Assert.AreEqual(1986, result[0].Result.BirthDate.Year);
            Assert.AreEqual(5, result[0].Result.BirthDate.Month);
            Assert.AreEqual(12, result[0].Result.BirthDate.Day);

            Assert.AreEqual("Max", result[1].Result.FirstName);
            Assert.AreEqual("Mustermann", result[1].Result.LastName);
            Assert.AreEqual(2014, result[1].Result.BirthDate.Year);
            Assert.AreEqual(1, result[1].Result.BirthDate.Month);
            Assert.AreEqual(1, result[1].Result.BirthDate.Day);
        }

        [Test]
        public void ReadFromStream_null_Test()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';', 1, true);
            ICsvMapping<Person> csvMapper = new CsvPersonMapping();
            CsvParser<Person> csvParser = new CsvParser<Person>(csvParserOptions, csvMapper);

            Assert.Throws<ArgumentNullException>(() => csvParser.ReadFromStream(null, Encoding.UTF8));
        }

        [Test]
        public void ReadFromStreamTest()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';', 1, true);
            ICsvMapping<Person> csvMapper = new CsvPersonMapping();
            CsvParser<Person> csvParser = new CsvParser<Person>(csvParserOptions, csvMapper);

            var stringBuilder = new StringBuilder()
                .AppendLine("FirstName;LastName;BirthDate")
                .AppendLine("     Philipp;Wagner;1986/05/12       ")
                .AppendLine("Max;Mustermann;2014/01/01");
#if NETCOREAPP1_1
            var basePath = AppContext.BaseDirectory;
#else 
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
#endif
            var filePath = Path.Combine(basePath, "test_file.txt");

            File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);

            using (var stream = File.OpenRead(filePath))
            {
                var result = csvParser
                    .ReadFromStream(stream, Encoding.UTF8)
                    .ToList();

                Assert.AreEqual(2, result.Count);

                Assert.IsTrue(result.All(x => x.IsValid));

                Assert.AreEqual("Philipp", result[0].Result.FirstName);
                Assert.AreEqual("Wagner", result[0].Result.LastName);

                Assert.AreEqual(1986, result[0].Result.BirthDate.Year);
                Assert.AreEqual(5, result[0].Result.BirthDate.Month);
                Assert.AreEqual(12, result[0].Result.BirthDate.Day);

                Assert.AreEqual("Max", result[1].Result.FirstName);
                Assert.AreEqual("Mustermann", result[1].Result.LastName);
                Assert.AreEqual(2014, result[1].Result.BirthDate.Year);
                Assert.AreEqual(1, result[1].Result.BirthDate.Month);
                Assert.AreEqual(1, result[1].Result.BirthDate.Day);
            }
        }
    }
}
