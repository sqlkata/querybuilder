using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SqlKata.Tests.Infrastructure
{
    public class TestSqlResultContainer : ReadOnlyDictionary<string, SqlResult>
    {
        public TestSqlResultContainer(IDictionary<string, SqlResult> dictionary) : base(dictionary)
        {

        }
    }
}