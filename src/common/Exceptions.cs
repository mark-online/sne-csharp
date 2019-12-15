using System;

namespace sne
{
    [Serializable]
    public sealed class StreamException : Exception
    {
        public StreamException() : base() {}

        public StreamException(string message) : base(message) { }
    }

    [Serializable]
    public sealed class MessageException : Exception
    {
        public MessageException() : base() { }

        public MessageException(string message) : base(message) { }
    }

} // sne
