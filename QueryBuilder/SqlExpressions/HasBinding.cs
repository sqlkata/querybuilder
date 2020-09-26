using System.Collections.Generic;

namespace SqlKata.SqlExpressions
{
    public interface HasBinding
    {
        IEnumerable<object> GetBindings();
    }
}