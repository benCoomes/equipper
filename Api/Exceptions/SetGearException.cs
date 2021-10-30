using System;
using System.Runtime.Serialization;

namespace Coomes.Equipper
{
    [Serializable]
    public class SetGearException : Exception
    {
        public SetGearException() { }
        public SetGearException(string message) : base(message) { }
        public SetGearException(string message, Exception innerException) : base(message, innerException) { }
        protected SetGearException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}