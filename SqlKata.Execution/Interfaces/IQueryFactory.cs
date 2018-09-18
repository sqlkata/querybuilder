using System;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Interfaces;

namespace SqlKata.Execution.Interfaces
{
    public interface IQueryFactory
    {
        Compiler Compiler { get; set; }
        Action<SqlResult> Logger { get; set; }
        IDbConnection Connection { get; set; }
        int QueryTimeout { get; set; }

        IQuery FromQuery(IQuery query);
        IQuery Query();
        IQuery Query(string table);
    }
}