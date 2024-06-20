using System;
using System.Runtime.Serialization;

namespace SqlKata.Exceptions.CreateTableQuery
{
    public class InvalidQueryMethodException : Exception
    {
        public InvalidQueryMethodException()
        {
        }

        public InvalidQueryMethodException(string message) : base(message)
        {
        }

        public InvalidQueryMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidQueryMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
