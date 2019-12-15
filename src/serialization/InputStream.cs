using System;
using System.Collections.Generic;
using System.Text;

namespace sne
{

    // System.BinaryReader 코드를 참고
    public class InputStream : ByteStream
    {
        private StreamBuffer _buffer;
        private byte[] _bytes = new byte[256];

        public InputStream() {
            _buffer = null;
        }

        public InputStream(StreamBuffer buffer) {
            _buffer = buffer;
        }

        public override bool isInput() {
            return true;
        }

        public void replace(StreamBuffer buffer) {
            _buffer = buffer;
        }

        public bool readBool() {
            return _buffer.read() != 0 ? true : false;
        }

        public byte readByte() {
            return _buffer.read();
        }

        public sbyte readSByte() {
            return (sbyte)_buffer.read();
        }

        public Int16 readInt16() {
            fillBytes(2);
            return (Int16)(_bytes[0] | (_bytes[1] << 8));
        }

        public UInt16 readUInt16() {
            fillBytes(2);
            return (UInt16)(_bytes[0] | (_bytes[1] << 8));
        }

        public Int32 readInt32() {
            fillBytes(4);
            return (Int32)(_bytes[0] | (_bytes[1] << 8) | (_bytes[2] << 16) | (_bytes[3] << 24));
        }

        public UInt32 readUInt32() {
            fillBytes(4);
            return (UInt32)(_bytes[0] | (_bytes[1] << 8) | (_bytes[2] << 16) | (_bytes[3] << 24));
        }

        public Int64 readInt64() {
            fillBytes(8);
            UInt32 mBuffer = (UInt32)(_bytes[0] | _bytes[1] << 8 | _bytes[2] << 16 | _bytes[3] << 24);
            UInt32 num = (UInt32)(_bytes[4] | _bytes[5] << 8 | _bytes[6] << 16 | _bytes[7] << 24);
            return (long)((ulong)num << 32 | (ulong)mBuffer);
        }

        public UInt64 readUInt64() {
            fillBytes(8);
            UInt32 mBuffer = (UInt32)(_bytes[0] | _bytes[1] << 8 | _bytes[2] << 16 | _bytes[3] << 24);
            UInt32 num = (UInt32)(_bytes[4] | _bytes[5] << 8 | _bytes[6] << 16 | _bytes[7] << 24);
            return (ulong)num << 32 | (ulong)mBuffer;
        }

        public Single readSingle() {
            fillBytes(4);
            return BitConverter.ToSingle(_bytes, 0); // from Lidgren
            //UInt32 value = (UInt32)(_bytes[0] | (_bytes[1] << 8) | (_bytes[2] << 16) | (_bytes[3] << 24));
            //return (float)value;
        }

        public byte[] readBytes(int count) {
            fillBytes(count);
            return _bytes;
        }

        public String readString() {
            int length = readUInt16();
            if (length > 0) {
                fillBytes(length);
                return System.Text.Encoding.UTF8.GetString(_bytes, 0, length);
            }
            return "";
        }

        public void readObject(ref object obj, Type type) {
            if (type == typeof(bool)) {
                obj = readBool();
            }
            else if (type == typeof(byte)) {
                obj = readByte();
            }
            else if (type == typeof(sbyte)) {
                obj = readSByte();
            }
            else if (type == typeof(Int16)) {
                obj = readInt16();
            }
            else if (type == typeof(UInt16)) {
                obj = readUInt16();
            }
            else if (type == typeof(Int32)) {
                obj = readInt32();
            }
            else if (type == typeof(UInt32)) {
                obj = readUInt32();
            }
            else if (type == typeof(Int64)) {
                obj = readInt64();
            }
            else if (type == typeof(UInt64)) {
                obj = readUInt64();
            }
            else if (type == typeof(Single)) {
                obj = readSingle();
            }
            else if (type == typeof(string)) {
                obj = readString();
            }
            else if (type.BaseType == typeof(Array)) {
                obj = ((Array)obj).serialize(this);
            }
            else {
                ByteStream.Serializer serializer = getSerializer(type);
                if (serializer != null) {
                    serializer(this, ref obj);
                    return;
                }

                var streamable = (obj as IStreamable);
                if (streamable != null) {
                    streamable.serialize(this);
                    return;
                }

                throw new StreamException(string.Format("Can't read object {0}", type.ToString()));
            }
        }

        public void reset() {
            _buffer.reset();
        }

        public int size() {
            return _buffer.size();
        }

        private void fillBytes(int numBytes) {
            if (numBytes < 0) {
                throw new StreamException();
            }

            if (_bytes.Length < numBytes) {
                _bytes = new byte[numBytes];
            }

            _buffer.copyTo(_bytes, numBytes);
        }
    }

} // namespace sne
