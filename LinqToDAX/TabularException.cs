using System;
using System.Runtime.Serialization;

namespace LinqToDAX
{
    [Serializable]
    public class TabularException : Exception
    {
        public TabularException() : base() { }

        public TabularException(string message) : base(message)
        {
        }

        public TabularException(string message, Exception innerException) : base(message,innerException)
        {
        }

        protected TabularException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) { }
    }
}