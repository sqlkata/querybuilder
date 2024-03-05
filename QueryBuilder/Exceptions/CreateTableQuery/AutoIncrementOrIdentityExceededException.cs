using System;
using System.Runtime.Serialization;

namespace SqlKata.Exceptions.CreateTableQuery
{
    public class AutoIncrementOrIdentityExceededException : Exception
    {
        public AutoIncrementOrIdentityExceededException()
        {
        }

        public AutoIncrementOrIdentityExceededException(string message) : base(message)
        {
        }

        public AutoIncrementOrIdentityExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AutoIncrementOrIdentityExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
