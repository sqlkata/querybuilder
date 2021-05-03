using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using SqlKata.Compilers;

namespace SqlKata
{
    public class TransactionWrapper
    {
        private IDbConnection Connection { get; set; }
        private Compiler Compiler { get; set; }
        public IDbTransaction Transaction { get; set; }

        public TransactionWrapper(IDbConnection connection, Compiler compiler)
        {
            this.Connection = connection;
            this.Compiler = compiler;
        }

        public void BeginTransaction()
        {
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        public void Rollback()
        {
            Transaction.Rollback();
            Connection.Close();
            Transaction.Dispose();
            Transaction = null;
        }

        public void Commit()
        {
            Transaction.Commit();
            Connection.Close();
            Transaction.Dispose();
            Transaction = null;
        }
    }
}
