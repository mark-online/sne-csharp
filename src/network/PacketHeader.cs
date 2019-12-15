using System;

namespace sne
{

    public class PacketHeader
    {
        // body-size + msg-type
        public const int packetHeaderSize = sizeof(UInt16) + sizeof(byte);

        UInt16 _bodySize;
        MessageType.Value _messageType;

        InputStream _inputStream;

        public UInt16 bodySize { get { return _bodySize; } }
        public MessageType.Value messageType { get { return _messageType; } }

        public PacketHeader(StreamBuffer buffer) {
            _inputStream = new InputStream(buffer);
            reset();
        }

        public void reset() {
            _bodySize = 0;
            _messageType = MessageType.Value.csmtUnknown;
        }

        public bool parse() {
            if (isValid()) {
                return true; // 이미 헤더를 받은 경우
            }

            if (_inputStream.size() < packetHeaderSize) {
                return false;
            }

            _bodySize = _inputStream.readUInt16();
            _messageType = (MessageType.Value)_inputStream.readByte();
            return true;
        }

        public bool isPacketHeaderArrived() {
            return _inputStream.size() >= packetHeaderSize;
        }

        public bool isMessageArrived() {
            return _inputStream.size() >= _bodySize;
        }

        public bool isValid() {
            return (_bodySize >= 0) && MessageType.isValidMessage(_messageType);
        }
    }

}
