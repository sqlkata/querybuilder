using System.Collections;
using System.Globalization;

namespace SqlKata
{
    public class SqlResult
    {
        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong)
        };

        public Dictionary<string, object?> NamedBindings = new();
        public required Query? Query { get; init; }
        public string RawSql { get; set; } = "";
        public List<object?> Bindings { get; set; } = new();
        public string Sql { get; set; } = "";

        public override string ToString()
        {
            var deepParameters = Helper.Flatten(Bindings).ToList();

            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                if (i >= deepParameters.Count)
                    throw new Exception(
                        $"Failed to retrieve a binding at index {i}, the total bindings count is {Bindings.Count}");

                return ChangeToSqlValue(deepParameters[i]);
            });
        }

        private static string ChangeToSqlValue(object? value)
        {
            if (value == null) return "NULL";

            if (Helper.IsArray(value))
                return Helper.JoinArray(",", (IEnumerable)value);

            if (NumberTypes.Contains(value.GetType()))
                return Convert.ToString(value, CultureInfo.InvariantCulture)!;

            if (value is DateTime date)
            {
                if (date.Date == date) return "'" + date.ToString("yyyy-MM-dd") + "'";

                return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }

            if (value is bool vBool) return vBool ? "true" : "false";

            if (value is Enum vEnum) return Convert.ToInt32(vEnum) + $" /* {vEnum} */";

            // fallback to string
            return "'" + value.ToString()!.Replace("'", "''") + "'";
        }
    }
}
