using System;
using System.Runtime.Serialization;

namespace Coomes.Equipper
{
    [Serializable]
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() { }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
        protected UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}