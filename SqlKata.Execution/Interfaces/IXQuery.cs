using System;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Interfaces;

namespace SqlKata.Execution.Interfaces
{
    public interface IXQuery
    {
        Compiler Compiler { get; set; }
        IDbConnection Connection { get; set; }
        Action<SqlResult> Logger { get; set; }

        IQuery Clone();
    }
}