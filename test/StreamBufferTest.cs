using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sne;

namespace SneCSharpUnitTest
{

    [TestClass]
    public class StreamBufferTest
    {
        const int capacity = 10;

        StreamBuffer _buffer = null;

        [TestInitialize()]
        public void Initialize() {
            _buffer = new StreamBuffer(capacity);
        }

        [TestMethod]
        public void Test_Initialize() {
            Assert.IsNotNull(_buffer);
        }

        [TestMethod]
        public void Test_Emtpy() {
            Assert.IsTrue(_buffer.empty(), "zero length");
            Assert.AreEqual<int>(0, _buffer.size(), "zero length");
        }

        [TestMethod]
        public void Test_EmptySpace() {
            Assert.AreEqual<int>(capacity, _buffer.space());
        }

        [TestMethod]
        public void Test_Write() {
            _buffer.write(1);
            Assert.AreEqual<int>(1, _buffer.size());
            _buffer.write(2);
            Assert.AreEqual<int>(2, _buffer.size());
        }

        [TestMethod]
        public void Test_Read() {
            _buffer.write(1);
            _buffer.write(2);
            byte value = _buffer.read();
            Assert.AreEqual<byte>(1, value);
            Assert.AreEqual<byte>(2, _buffer.read());
            Assert.IsTrue(_buffer.empty(), "zero length");

            _buffer.write(3);
            Assert.AreEqual<int>(1, _buffer.size());
        }

        [TestMethod]
        public void Test_Reset() {
            _buffer.write(1);
            _buffer.write(2);
            _buffer.reset();
            Assert.IsTrue(_buffer.empty(), "zero length");
            Assert.AreEqual<int>(0, _buffer.size());
        }

        [TestMethod]
        public void Test_Space() {
            Assert.AreEqual<int>(capacity, _buffer.space());

            _buffer.write(1);
            _buffer.write(2);
            Assert.AreEqual<int>(capacity - 2, _buffer.space());

            _buffer.read();
            _buffer.read();
            Assert.AreEqual<int>(capacity, _buffer.space());
        }

        [TestMethod]
        public void Test_ExtendedSpace() {
            Assert.AreEqual<int>(capacity, _buffer.space());

            for (int i = 0; i < (capacity * 2); ++i) {
                _buffer.write((byte)i);
            }
            Assert.AreEqual<int>(0, _buffer.space());

            _buffer.reset();
            Assert.AreEqual<int>(capacity * 2, _buffer.space());
        }

        [TestMethod]
        public void Test_CopyFrom() {
            int intValue = 1234567890;
            byte[] bytes = BitConverter.GetBytes(intValue);
            _buffer.copyFrom(bytes, 0, bytes.Length);
            Assert.AreEqual<int>(sizeof(int), _buffer.size());
        }

        [TestMethod]
        public void Test_CopyTo() {
            int intValue = 1234567890;
            byte[] inMemory = BitConverter.GetBytes(intValue);
            _buffer.copyFrom(inMemory, 0, inMemory.Length);
            Assert.AreEqual<int>(sizeof(int), _buffer.size());

            byte[] outMemory = new byte[inMemory.Length];
            _buffer.copyTo(outMemory, sizeof(int));
            Assert.IsTrue(_buffer.empty());
            Assert.AreEqual<int>(0, _buffer.size());
            Assert.IsTrue(Enumerable.SequenceEqual(inMemory, outMemory));
        }

        [TestMethod]
        [ExpectedException(typeof(StreamException))]
        public void Test_ExceptionByEmptyRead() {
            _buffer.read();
        }

        [TestMethod]
        [ExpectedException(typeof(StreamException))]
        public void Test_ExceptionByCopyTo() {
            byte[] outMemory = new byte[100];
            _buffer.copyTo(outMemory, 100);
        }

        [TestMethod]
        public void Test_Crunch() {
            _buffer.write(1);
            _buffer.write(2);
            _buffer.read();
            Assert.AreEqual<int>(1, _buffer.readIndex);
            Assert.AreEqual<int>(2, _buffer.writeIndex);

            _buffer.crunch();
            Assert.AreEqual<int>(1, _buffer.size());
            Assert.AreEqual<int>(0, _buffer.readIndex);
            Assert.AreEqual<int>(1, _buffer.writeIndex);
        }
    }

} // SneCSharpUnitTest
