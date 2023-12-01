using System;

namespace QuarterMaster.Communications
{
    [Serializable]
    internal class ExtendedIMAPException : Exception
    {
        public ExtendedIMAPException(string message) : base(message) => new Exception(message);

        public ExtendedIMAPException()
        {
        }

        public ExtendedIMAPException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExtendedIMAPException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
