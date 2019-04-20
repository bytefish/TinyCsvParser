// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NUnit.Framework;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace TinyCsvParser.Test.Mapping
{

    [TestFixture]
    public class CsvMappingTests
    {
        private class SampleEntity
        {
            public int PropertyInt { get; set; }
        }

        private class DuplicateMapping : CsvMapping<SampleEntity>, ICsvMapping<SampleEntity>
        {
            public DuplicateMapping()
            {
                MapProperty(0, x => x.PropertyInt);
                MapProperty(0, x => x.PropertyInt);
            }
        }
        
        [Test]
        public void DuplicateMappingTest()
        {
            Assert.Throws<InvalidOperationException>(() => new DuplicateMapping());
        }

        private class WrongColumnMapping : CsvMapping<SampleEntity>, ICsvMapping<SampleEntity>
        {
            public WrongColumnMapping()
            {
                MapProperty(2, x => x.PropertyInt);
            }
        }

        [Test]
        public void MapEntity_Invalid_Column_Test()
        {
            var mapping = new WrongColumnMapping();

            var result = mapping.Map(new TokenizedRow(1, new[] { "A", "1" }));

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual("A|1", result.Error.UnmappedRow);
        }

        private class CorrectColumnMapping : CsvMapping<SampleEntity>, ICsvMapping<SampleEntity>
        {
            public CorrectColumnMapping()
            {
                MapProperty(0, x => x.PropertyInt);
            }
        }
        

        [Test]
        public void MapEntity_ConversionError_Test()
        {
            var mapping = new CorrectColumnMapping();

            var result = mapping.Map(new TokenizedRow(1, new[] { string.Empty }));

            Assert.IsFalse(result.IsValid);

            Assert.AreEqual("Column 0 with Value '' cannot be converted", result.Error.Value);
            Assert.AreEqual(0, result.Error.ColumnIndex);
            Assert.AreEqual(string.Empty, result.Error.UnmappedRow);

            Assert.DoesNotThrow(() => result.ToString());
        }

        [Test]
        public void MapEntity_ConversionSuccess_Test()
        {
            var mapping = new CorrectColumnMapping();

            var result = mapping.Map(new TokenizedRow(1, new[] { "1" }));

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(1, result.Result.PropertyInt);

            Assert.DoesNotThrow(() => result.ToString());
        }
    }
}
