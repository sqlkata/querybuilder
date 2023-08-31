using System.Collections.Immutable;
using SqlKata.Compilers;

namespace SqlKata
{
    public static class InsertColumnsExt
    {
        public static string GetInsertColumnsList(this ImmutableArray<string> columns ,X x)
        {
            return !columns.Any()
                ? ""
                : $" ({string.Join(", ", columns.Select(x.Wrap))})";
        }
        public static void WriteInsertColumnsList(this Writer writer, ImmutableArray<string> columns, bool braces = true)
        {
            if (columns.Length == 0) return;
            if (braces) writer.Append(" (");
            writer.List(", ", columns, writer.AppendName);
            if (braces) writer.Append(")");
        }
    }
    public abstract class AbstractInsertClause : AbstractClause
    {
    }

    public class InsertClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required ImmutableArray<object?> Values { get; init; }
        public required bool ReturnId { get; init; }
    }

    public sealed class InsertQueryClause : AbstractInsertClause
    {
        public required ImmutableArray<string> Columns { get; init; }
        public required Query Query { get; init; }
    }
}
