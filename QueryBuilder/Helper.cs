using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public static class Helper
    {
        public static bool IsArray(object value)
        {
            if (value is string)
                return false;

            return value is IEnumerable;
        }

        public static IEnumerable<object> Flatten(IEnumerable<object> array)
        {
            return array.SelectMany(o => IsArray(o) ? Flatten(o as IEnumerable<object>) : new[] { o });
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

            var splitted = subject.Split(new[] { match }, StringSplitOptions.None);
            return splitted
                .Skip(1)
                .Select((item, index) => callback(index) + item)
                .Aggregate(splitted.First(), (left, right) => left + right);
        }
    }
}