using System.Collections.Generic;
using SqlKata.Execution;

namespace SqlKata.Tests.Infrastructure;

static class QueryFactoryExtensions
{
    public static QueryFactory Create(this QueryFactory db, string table, IEnumerable<string> cols)
    {
        db.Drop(table);
        db.Statement($"CREATE TABLE `{table}`({string.Join(", ", cols)});");
        return db;
    }

    public static QueryFactory Drop(this QueryFactory db, string table)
    {
        db.Statement($"DROP TABLE IF EXISTS `{table}`;");
        return db;
    }
}
