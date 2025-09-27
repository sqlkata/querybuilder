namespace SqlKata.Tests.Infrastructure;

using System.Collections.Generic;
using System.Collections.ObjectModel;

public class TestSqlResultContainer : ReadOnlyDictionary<string, SqlResult> {
    public TestSqlResultContainer(IDictionary<string, SqlResult> dictionary) : base(dictionary) {

    }
}
