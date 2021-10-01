
namespace Coomes.Equipper
{
    // todo: translate into 400 response
    public class BadRequestException : System.Exception
    {
        public BadRequestException() { }
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, System.Exception inner) : base(message, inner) { }
        protected BadRequestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}