using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace sne
{

    /// <summary>
    /// Utility struct for writing Singles
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SingleUIntUnion
    {
        /// <summary>
        /// Value as a 32 bit float
        /// </summary>
        [FieldOffset(0)]
        public float SingleValue;

        /// <summary>
        /// Value as an unsigned 32 bit integer
        /// </summary>
        [FieldOffset(0)]
        //[CLSCompliant(false)]
        public UInt32 UIntValue;
    }

    // System.BinaryWriter 코드를 참고
    // - TODO: endian 처리
    public class OutputStream : ByteStream
    {
        private StreamBuffer _buffer;
        private byte[] _bytes = new byte[8];

        public StreamBuffer buffer { get { return _buffer; } }

        public OutputStream() {
            _buffer = null;
        }

        public OutputStream(StreamBuffer buffer) {
            _buffer = buffer;
        }

        public override bool isOutput() {
            return true;
        }

        public void replace(StreamBuffer buffer) {
            _buffer = buffer;
        }

        public void write(bool value) {
            _buffer.write((byte)(value ? 1 : 0));
        }

        public void write(byte value) {
            _buffer.write(value);
        }

        public void write(sbyte value) {
            _buffer.write((byte)value);
        }

        public void write(Int16 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _buffer.copyFrom(_bytes, 0, 2);
        }

        public void write(UInt16 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _buffer.copyFrom(_bytes, 0, 2);
        }

        public void write(Int32 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _bytes[2] = (byte)(value >> 16);
            _bytes[3] = (byte)(value >> 24);
            _buffer.copyFrom(_bytes, 0, 4);
        }

        public void write(UInt32 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _bytes[2] = (byte)(value >> 16);
            _bytes[3] = (byte)(value >> 24);
            _buffer.copyFrom(_bytes, 0, 4);
        }

        public void write(Int64 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _bytes[2] = (byte)(value >> 16);
            _bytes[3] = (byte)(value >> 24);
            _bytes[4] = (byte)(value >> 32);
            _bytes[5] = (byte)(value >> 40);
            _bytes[6] = (byte)(value >> 48);
            _bytes[7] = (byte)(value >> 56);
            _buffer.copyFrom(_bytes, 0, 8);
        }

        public void write(UInt64 value) {
            _bytes[0] = (byte)value;
            _bytes[1] = (byte)(value >> 8);
            _bytes[2] = (byte)(value >> 16);
            _bytes[3] = (byte)(value >> 24);
            _bytes[4] = (byte)(value >> 32);
            _bytes[5] = (byte)(value >> 40);
            _bytes[6] = (byte)(value >> 48);
            _bytes[7] = (byte)(value >> 56);
            _buffer.copyFrom(_bytes, 0, 8);
        }

        public void write(Single value) {
            //byte[] bytes = BitConverter.GetBytes(value);
            //_buffer.copyFrom(bytes, 0, 4);

            // from Lidgren
            // Use union to avoid BitConverter.GetBytes() which allocates memory on the heap
            SingleUIntUnion su;
            su.UIntValue = 0; // must initialize every member of the union to avoid warning
            su.SingleValue = value;
            write(su.UIntValue);

            //unsafe {
            //    UInt32 num = (UInt32)value;
            //    _bytes[0] = (byte)num;
            //    _bytes[1] = (byte)(num >> 8);
            //    _bytes[2] = (byte)(num >> 16);
            //    _bytes[3] = (byte)(num >> 24);
            //    _buffer.copyFrom(_bytes, 0, 4);
            //}
        }

        public void write(byte[] value, int count) {
            _buffer.copyFrom(value, 0, count);
        }

        public void write(string value) {
            if (value.Length > 0) {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                write((UInt16)bytes.Length);
                write(bytes, bytes.Length);
            }
            else {
                write((UInt16)0);
            }
        }

        public void write(Array value) {
            value.serialize(this);
        }

        public void writeObject(object obj) {
            System.Type type = obj.GetType();
            if (type == typeof(bool)) {
                write((bool)obj);
            }
            else if (type == typeof(byte)) {
                write((byte)obj);
            }
            else if (type == typeof(sbyte)) {
                write((sbyte)obj);
            }
            else if (type == typeof(Int16)) {
                write((Int16)obj);
            }
            else if (type == typeof(UInt16)) {
                write((UInt16)obj);
            }
            else if (type == typeof(Int32)) {
                write((Int32)obj);
            }
            else if (type == typeof(UInt32)) {
                write((UInt32)obj);
            }
            else if (type == typeof(Int64)) {
                write((Int64)obj);
            }
            else if (type == typeof(UInt64)) {
                write((UInt64)obj);
            }
            else if (type == typeof(Single)) {
                write((Single)obj);
            }
            else if (type == typeof(string)) {
                write((string)obj);
            }
            else if (type.BaseType == typeof(Array)) {
                write((Array)obj);
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

                throw new StreamException(string.Format("Can't write object {0}", type.ToString()));
            }
        }

        public void reset() {
            _buffer.reset();
        }

        public int size() {
            return _buffer.size();
        }
    }

} // namespace sne
