
namespace sne
{
    static public class MessageType
    {
        /// C/S 전용 메세지 타입
        public enum Value
        {
            csmtUnknown = -1,

            mtFirst = 0,
            mtLast = 255,

            // = system messages
            csmtSystemFirst = 0,
            csmtSystem_heartbeat = csmtSystemFirst,
            csmtSystem_exchangeSeed,
            csmtSystem_exchangePublicKey,
            csmtSystem_confirmSeed,
            csmtSystemLast,

            // = user defined messages
            csmtUserDefinedBegin = 21,
            csmtRpc = csmtUserDefinedBegin,
            csmtUserDefinedLast,
        }

        static public bool isValidMessage(Value mt) {
            return (Value.mtFirst <= mt) && (mt <= Value.mtLast);
        }

        static public bool isSystemMessage(Value mt) {
            return (Value.csmtSystemFirst <= mt) && (mt < Value.csmtSystemLast);
        }

        static public bool isRpcMessage(Value mt) {
            return Value.csmtRpc == mt;
        }

        static public Value toMessageType(int mt) {
            return (Value)mt;
        }
    }


    public abstract class Message : IStreamable
    {
        public abstract MessageType.Value getMessageType();

        public abstract void serialize(ByteStream stream);
    }

    #region System messages

    public class HeartbeatMessage : Message
    {
        public string text { get; set; }

        public override MessageType.Value getMessageType() {
            return MessageType.Value.csmtSystem_heartbeat;
        }

        public override void serialize(ByteStream stream) {
            // NOP
        }
    }

    #endregion // System messages

} // namespace sne
