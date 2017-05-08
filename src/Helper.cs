using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SqlKata
{
    public static class Helper
    {
        public static Raw Raw(string value)
        {
            return new Raw(value);
        }
        public static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        public static bool IsArray(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is string)
            {
                return false;
            }

            return value is IEnumerable;
        }

        public static List<object> Flatten(IEnumerable array)
        {
            var result = new List<object>();

            foreach (var item in array)
            {
                if (IsArray(item))
                {
                    result.AddRange(Flatten((IEnumerable)item));
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {

                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }
    }
}