using SqlKata.Compilers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlKata
{
    public static class Helper
    {
        public static bool IsArray(object value)
        {
            if(value is string)
            {
                return false;
            }

            if (value is byte[])
            {
                return false;
            }

            return value is IEnumerable;
        }

        /// <summary>
        /// Flat IEnumerable one level down
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<object> Flatten(IEnumerable<object> array)
        {
            foreach (var item in array)
            {
                if (IsArray(item))
                {
                    foreach (var sub in (item as IEnumerable))
                    {
                        yield return sub;
                    }
                }
                else
                {
                    yield return item;
                }

            }
        }

        public static IEnumerable<object> FlattenDeep(IEnumerable<object> array)
        {
            return array.SelectMany(o => IsArray(o) ? FlattenDeep(o as IEnumerable<object>) : new[] { o });
        }

        public static IEnumerable<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                yield break;
            }

            var index = 0;

            do
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);

                if (index == -1)
                {
                    yield break;
                }

                yield return index;

            } while ((index += value.Length) < str.Length);
        }

        public static string ReplaceAll(string subject, string match, Func<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(subject) || !subject.Contains(match))
            {
                return subject;
            }

            var splitted = subject.Split(
                new[] { match },
                StringSplitOptions.None
            );

            return splitted.Skip(1)
                .Select((item, index) => callback(index) + item)
                .Aggregate(splitted.First(), (left, right) => left + right);
        }

        public static string JoinArray(string glue, IEnumerable array)
        {
            var result = new List<string>();

            foreach (var item in array)
            {
                result.Add(item.ToString());
            }

            return string.Join(glue, result);
        }

        public static string ExpandParameters(string sql, string placeholder, object[] bindings)
        {
            return ReplaceAll(sql, placeholder, i =>
            {
                var parameter = bindings[i];

                if (IsArray(parameter))
                {
                    var count = EnumerableCount(parameter as IEnumerable);
                    return string.Join(",", placeholder.Repeat(count));
                }

                return placeholder.ToString();
            });
        }

        public static int EnumerableCount(IEnumerable obj)
        {
            int count = 0;

            foreach (var item in obj)
            {
                count++;
            }

            return count;
        }

        public static List<string> ExpandExpression(string expression)
        {
            var regex = @"^(?:\w+\.){1,2}{(.*)}";
            var match = Regex.Match(expression, regex);

            if (!match.Success)
            {
                // we did not found a match return the string as is.
                return new List<string> { expression };
            }

            var table = expression.Substring(0, expression.IndexOf(".{"));

            var captures = match.Groups[1].Value;

            var cols = Regex.Split(captures, @"\s*,\s*")
                .Select(x => $"{table}.{x.Trim()}")
                .ToList();

            return cols;
        }

        public static IEnumerable<string> Repeat(this string str, int count)
        {
            return Enumerable.Repeat(str, count);
        }
        
        public static string ReplaceIdentifierUnlessEscaped(this string input, string escapeCharacter, string identifier, string newIdentifier)
        {
            //Replace standard, non-escaped identifiers first
            var nonEscapedRegex = new Regex($@"(?<!{Regex.Escape(escapeCharacter)}){Regex.Escape(identifier)}");
            var nonEscapedReplace = nonEscapedRegex.Replace(input, newIdentifier);
            
            //Then replace escaped identifiers, by just removing the escape character
            var escapedRegex = new Regex($@"{Regex.Escape(escapeCharacter)}{Regex.Escape(identifier)}");
            return escapedRegex.Replace(nonEscapedReplace, identifier);

        }

       
    }


    public static  class ConvertHelper
    {
        //https://stackoverflow.com/questions/425389/c-sharp-equivalent-of-sql-server-datatypes
        private static readonly Dictionary<string, string> DotNetMappingSqlServer = new Dictionary<string, string> {
             { "Byte[]", "VARBINARY" } ,
             { "Byte", "BINARY" } ,
             { "Boolean", "BIT" } ,
             { "Char", "nvarchar(1)" } ,
             { "DateTime", "DATETIME" } ,
             { "Decimal", "DECIMAL" } ,
             { "Double", "FLOAT" } ,
             {"Int16", "smallint"},
             {"Int32", "Int"},
             {"Int64", "bigint"},
             { "Single", "REAL" } ,
             { "object", "SQL_VARIANT" } ,
             { "Guid", "UNIQUEIDENTIFIER" } ,
             { "string", "NVARCHAR" } ,
             };


        /// Mapping of .NET Framework Data Types to Oracle Native Data Types https://docs.microsoft.com/en-us/dotnet/api/system.data.oracleclient.oracletype?redirectedfrom=MSDN&view=netframework-4.8    and https://docs.oracle.com/html/B10961_01/features.htm#1024984 and https://docs.oracle.com/cd/B19306_01/win.102/b14306/appendixa.htm
        private static readonly Dictionary<string, string> DotNetMappingOracle = new Dictionary<string, string> {
            {"Byte","Byte"},
            {"Byte[]", "Raw"},
            {"Char", "Varchar2"},
            {"Char[]", "Varchar2"},
            {"DateTime", "TimeStamp"},
            {"Decimal", "Decimal"},
            {"Double", "Double"},
            {"Float", "Single"},
            {"Int16", "Int16"},
            {"Int32", "Int32"},
            {"Int64", "Int64"},
            {"Single", "Single"},
            {"String", "Varchar2" },
            {"TimeSpan", "IntervalDS"},
        };


        //https://docs.telerik.com/data-access/developers-guide/database-specifics/sqlite/database-specifics-sqlite-type-mapping
        private static readonly Dictionary<string, string> DotNetMappingSqlite = new Dictionary<string, string>() {
            {"Boolean"  ,"BIT"},
            {"Char"     ,"CHAR(1)"},
            {"SByte"    ,"SMALLINT"},
            {"Byte"     ,"SMALLINT"},
            {"Int16"    ,"SMALLINT"},
            {"UInt16"   ,"INTEGER"},
            {"Int32"    ,"INTEGER"},
            {"UInt32"   ,"BIGINT"},
            {"Int64"    ,"BIGINT"},
            {"UInt64"   ,"UNSIGNED BIG INT"},
            {"Single"   ,"REAL"},
            {"Double"   ,"DOUBLE"},
            {"String"   ,"VARCHAR(255)"},
            {"DateTime" ,"TIMESTAMP"},
            {"Decimal"  ,"NUMERIC(20,10)"},
            {"Guid"     ,"GUID"},
            {"Byte[]"   ,"BLOB"},
        };


        //https://docs.telerik.com/data-access/developers-guide/database-specifics/firebird/database-specifics-firebird-type-mapping
        private static readonly Dictionary<string, string> DotNetMappingFireBird = new Dictionary<string, string>{
            {"Boolean",  "SMALLINT"},
            {"Char", "CHAR(1)"},
            {"SByte",    "SMALLINT"},
            {"Byte", "SMALLINT"},
            {"Int16",    "SMALLINT"},
            {"UInt16",   "INTEGER"},
            {"Int32",    "INTEGER"},
            {"UInt32",   "BIGINT"},
            {"Int64",    "BIGINT"},
            {"UInt64",   "BIGINT"},
            {"Single",   "FLOAT"},
            {"Double",   "DOUBLE PRECISION"},
            {"String",   "VARCHAR(190)"},
            {"DateTime", "TIMESTAMP"},
            {"Decimal",  "NUMERIC(18,8)"},
            {"Guid", "CHAR(16)"},
            {"Byte[]", "BLOB"},

        };


        //https://docs.telerik.com/data-access/developers-guide/database-specifics/postgresql/data-access-tasks-postgresql-type-mapping
        private static readonly Dictionary<string, string> DotNetMappingPostGreSQL = new Dictionary<string, string>
        {
            {"Boolean",  "BOOL"},
            {"Char", "BPCHAR(1)"},
            {"SByte",    "INT2"},
            {"Byte", "INT2"},
            {"Int16",    "INT2"},
            {"UInt16",   "INT4"},
            {"Int32",    "INT4"},
            {"UInt32",   "INT8"},
            {"Int64",    "INT8"},
            {"UInt64",   "NUMERIC(20)"},
            {"Single",   "FLOAT4"},
            {"Double",   "FLOAT8"},
            {"String",   "VARCHAR(255)"},
            {"DateTime", "TIMESTAMP"},
            {"Decimal",  "NUMERIC(20,10)"},
            {"Guid", "UUID"},
            {"Byte[]",  "BYTEA"},
        };

        /// <summary>
        /// Returns the 
        /// </summary>
        /// <param name="engineCode"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static string ConverToTypeSqlDataType(Type valueType, string engineCode)
        {

            var valueTypeName = valueType.Name;

            if (EngineCodes.SqlServer == engineCode)
            {
                if (DotNetMappingSqlServer.TryGetValue(valueTypeName, out string value))
                {
                    return value;
                }

            }
            else if (EngineCodes.Sqlite == engineCode)
            {
                if (DotNetMappingSqlite.TryGetValue(valueTypeName, out string value))
                {
                    return value;
                }
            }

            else if (EngineCodes.Oracle == engineCode)
            {
                if(DotNetMappingOracle.TryGetValue(valueTypeName, out string value))
                {
                    return value;
                }
            }
            else if(EngineCodes.Firebird == engineCode)
            {
                if(DotNetMappingFireBird.TryGetValue(valueTypeName, out string value))
                {
                    return value;
                }
            }
            else if (EngineCodes.PostgreSql == engineCode)
            {
                if (DotNetMappingPostGreSQL.TryGetValue(valueTypeName, out string value))
                {
                    return value;
                }
            }
            throw new NotSupportedException($"DataType {valueTypeName} not supported for the engine {engineCode}");

        }
    } 

}