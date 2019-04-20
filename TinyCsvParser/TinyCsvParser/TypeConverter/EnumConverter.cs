// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using TinyCsvParser.Reflection;

namespace TinyCsvParser.TypeConverter
{
    public class EnumConverter<TTargetType> : NonNullableConverter<TTargetType>
        where TTargetType : struct
    {
        private readonly Type enumType;
        private readonly bool ignoreCase;

        public EnumConverter()
            : this(true)
        {
        }

        public EnumConverter(bool ignoreCase)
        {
            if (!ReflectionUtils.IsEnum(typeof(TTargetType)))
            {
                throw new ArgumentException(string.Format("Type {0} is not a valid Enum", enumType));
            }
            this.enumType = typeof(TTargetType);
            this.ignoreCase = ignoreCase;
        }

        protected override bool InternalConvert(string value, out TTargetType result)
        {
            char charResult;
            if (Char.TryParse(value, out charResult))
            {
                if (Enum.IsDefined(typeof(TTargetType), (int)charResult))
                {
                    return Enum.TryParse<TTargetType>(((int)charResult).ToString(), ignoreCase, out result);
                }
            }
            return Enum.TryParse<TTargetType>(value, ignoreCase, out result);
        }
    }
}
