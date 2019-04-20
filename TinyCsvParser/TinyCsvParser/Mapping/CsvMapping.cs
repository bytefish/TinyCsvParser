// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using TinyCsvParser.TypeConverter;
using TinyCsvParser.Model;

namespace TinyCsvParser.Mapping
{
    public abstract class CsvMapping<TEntity> : ICsvMapping<TEntity>
            where TEntity : class, new()
    {
        private class IndexToPropertyMapping
        {
            public int ColumnIndex { get; set; }

            public ICsvPropertyMapping<TEntity> PropertyMapping { get; set; }

            public override string ToString()
            {
                return string.Format("IndexToPropertyMapping (ColumnIndex = {0}, PropertyMapping = {1}", ColumnIndex, PropertyMapping);
            }
        }

        private readonly ITypeConverterProvider typeConverterProvider;
        private readonly List<IndexToPropertyMapping> csvPropertyMappings;

        protected CsvMapping()
            : this(new TypeConverterProvider())
        {
        }

        protected CsvMapping(ITypeConverterProvider typeConverterProvider)
        {
            this.typeConverterProvider = typeConverterProvider;
            this.csvPropertyMappings = new List<IndexToPropertyMapping>();
        }

        protected CsvPropertyMapping<TEntity, TProperty> MapProperty<TProperty>(int columnIndex, Expression<Func<TEntity, TProperty>> property)
        {
            return MapProperty(columnIndex, property, typeConverterProvider.Resolve<TProperty>());
        }

        protected CsvPropertyMapping<TEntity, TProperty> MapProperty<TProperty>(int columnIndex, Expression<Func<TEntity, TProperty>> property, ITypeConverter<TProperty> typeConverter)
        {
            if (csvPropertyMappings.Any(x => x.ColumnIndex == columnIndex))
            {
                throw new InvalidOperationException(string.Format("Duplicate mapping for column index {0}", columnIndex));
            }

            var propertyMapping = new CsvPropertyMapping<TEntity, TProperty>(property, typeConverter);

            AddPropertyMapping(columnIndex, propertyMapping);

            return propertyMapping;
        }

        private void AddPropertyMapping<TProperty>(int columnIndex, CsvPropertyMapping<TEntity, TProperty> propertyMapping)
        {
            var indexToPropertyMapping = new IndexToPropertyMapping
            {
                ColumnIndex = columnIndex,
                PropertyMapping = propertyMapping
            };

            csvPropertyMappings.Add(indexToPropertyMapping);
        }

        public CsvMappingResult<TEntity> Map(TokenizedRow values)
        {
            TEntity entity = new TEntity();

            for (int pos = 0; pos < csvPropertyMappings.Count; pos++)
            {
                var indexToPropertyMapping = csvPropertyMappings[pos];

                var columnIndex = indexToPropertyMapping.ColumnIndex;

                if (columnIndex >= values.Tokens.Length)
                {
                    return new CsvMappingResult<TEntity>
                    {
                        RowIndex = values.Index,
                        Error = new CsvMappingError
                        {
                            ColumnIndex = columnIndex,
                            Value = $"Column {columnIndex} is Out Of Range",
                            UnmappedRow = string.Join("|", values.Tokens)
                        }
                    };
                }

                var value = values.Tokens[columnIndex];

                if (!indexToPropertyMapping.PropertyMapping.TryMapValue(entity, value))
                {
                    return new CsvMappingResult<TEntity>
                    {
                        RowIndex = values.Index,
                        Error = new CsvMappingError
                        {
                            ColumnIndex = columnIndex,
                            Value = $"Column {columnIndex} with Value '{value}' cannot be converted",
                            UnmappedRow = string.Join("|", values.Tokens)
                        }
                    };
                }
            }

            return new CsvMappingResult<TEntity>
            {
                RowIndex = values.Index,
                Result = entity
            };
        }


        public override string ToString()
        {
            var csvPropertyMappingsString = string.Join(", ", csvPropertyMappings.Select(x => x.ToString()));

            return $"CsvMapping (TypeConverterProvider = {typeConverterProvider}, Mappings = {csvPropertyMappingsString})";
        }
    }
}