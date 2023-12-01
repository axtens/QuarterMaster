using System;

namespace QuarterMaster.Communications
{
    /// <summary>Represents errors that occur during application execution.</summary>
    /// <filterpriority>1</filterpriority>
    public class IMAPException : Exception
    {
        public IMAPException()
        {
            new Exception();
        }

        public IMAPException(string message) : base(message)
        {
            new Exception(message);
        }

        public IMAPException(string message, Exception innerException) : base(message, innerException)
        {
            new Exception(message, innerException);
        }

        protected IMAPException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
