using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public static class Helper
    {
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
            if (value == null || value is string)
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

        public static bool IsGenericType(Type type)
        {
            return type
#if FEATURE_TYPE_INFO
            .GetTypeInfo()
#endif
            .IsGenericType;
        }

        public static Type BaseType(Type type)
        {
            return type
#if FEATURE_TYPE_INFO
            .GetTypeInfo()
#endif
            .BaseType;
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {

            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = IsGenericType(toCheck) ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == cur)
                    return true;

                toCheck = BaseType(toCheck);
            }

            return false;
        }

        public static IEnumerable<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                yield break;

            var index = 0;
            do
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);

                if (index == -1)
                    yield break;

                yield return index;
            } while ((index += value.Length) < str.Length);
        }

        public static string ReplaceAll(string subject, string match, Func<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match))
                return subject;

            var index = 0;
            return subject
                .Split(new[] { match }, StringSplitOptions.None)
                .Aggregate((left, right) => left + callback(index++) + right);
        }

    }
}