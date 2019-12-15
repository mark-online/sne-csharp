using System;
using sne;

namespace EchoClient
{
    enum EchoMessageType
    {
        emtEcho = MessageType.Value.csmtUserDefinedLast
    }

    public class EchoMessage : Message
    {
        public string text { get; set; }

        public override MessageType.Value getMessageType() {
            return MessageType.toMessageType((int)EchoMessageType.emtEcho);
        }

        public override void serialize(ByteStream stream) {
            if (stream.isInput()) {
                text = (stream as InputStream).readString();
            }
            else {
                (stream as OutputStream).write(text);
            }
        }
    }
}
