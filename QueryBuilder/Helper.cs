using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public static class Helper
    {
        #region IsNumber
        /// <summary>
        /// Returns <c>true</c> when <paramref name="value"/> is a number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        #endregion

        #region IsArray
        /// <summary>
        /// Returns <c>true</c> when <paramref name="value"/> is an array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsArray(object value)
        {
            if (value is string)
                return false;

            return value is IEnumerable;
        }
        #endregion

        public static IEnumerable<object> Flatten(IEnumerable<object> array)
        {
            return array.SelectMany(o => IsArray(o) ? Flatten(o as IEnumerable<object>) : new[] {o});
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="type"/> is a generic
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        #region ReplaceAll
        /// <summary>
        /// Replaces all <paramref name="match"/> in <paramref name="subject"/>
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="match"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static string ReplaceAll(string subject, string match, Func<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match))
                return subject;

            var splited = subject.Split(new[] { match }, StringSplitOptions.None);
            return splited
                .Skip(1)
                .Select((item, index) => callback(index) + item)
                .Aggregate(splited.First(), (left, right) => left + right);
        }
        #endregion
    }
}