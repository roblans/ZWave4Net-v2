using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave
{

    [Serializable]
    internal class OperationFailedException : Exception
    {
        public OperationFailedException() { }
        public OperationFailedException(string message) : base(message) { }
        public OperationFailedException(string message, Exception inner) : base(message, inner) { }
        protected OperationFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
