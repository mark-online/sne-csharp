using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sne;

namespace SneCSharpUnitTest
{
    [TestClass]
    public class OutputStreamTest
    {
        StreamBuffer _buffer = null;
        OutputStream _stream = null;

        [TestInitialize()]
        public void Initialize() {
            _buffer = new StreamBuffer(10);
            _stream = new OutputStream(_buffer);
        }

        [TestMethod]
        public void Test_Empty() {
            Assert.AreEqual<int>(0, _stream.size());
        }

        [TestMethod]
        public void Test_WriteByte() {
            _stream.write((byte)1);
            Assert.AreEqual<int>(1, _stream.size());
            _stream.write((byte)2);
            Assert.AreEqual<int>(2, _stream.size());
        }

        [TestMethod]
        public void Test_WriteSByte() {
            _stream.write((sbyte)1);
            Assert.AreEqual<int>(1, _stream.size());
            _stream.write((sbyte)2);
            Assert.AreEqual<int>(2, _stream.size());
        }

        [TestMethod]
        public void Test_WriteInt16() {
            _stream.write((Int16)1);
            Assert.AreEqual<int>(1 * sizeof(Int16), _stream.size());
            _stream.write((Int16)2);
            Assert.AreEqual<int>(2 * sizeof(Int16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteUInt16() {
            _stream.write((UInt16)1);
            Assert.AreEqual<int>(1 * sizeof(UInt16), _stream.size());
            _stream.write((UInt16)2);
            Assert.AreEqual<int>(2 * sizeof(UInt16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteInt32() {
            _stream.write((Int32)1);
            Assert.AreEqual<int>(1 * sizeof(Int32), _stream.size());
            _stream.write((Int32)2);
            Assert.AreEqual<int>(2 * sizeof(Int32), _stream.size());
        }

        [TestMethod]
        public void Test_WriteUInt32() {
            _stream.write((UInt32)1);
            Assert.AreEqual<int>(1 * sizeof(UInt32), _stream.size());
            _stream.write((UInt32)2);
            Assert.AreEqual<int>(2 * sizeof(UInt32), _stream.size());
        }

        [TestMethod]
        public void Test_WriteInt64() {
            _stream.write((Int64)1);
            Assert.AreEqual<int>(1 * sizeof(Int64), _stream.size());
            _stream.write((Int64)2);
            Assert.AreEqual<int>(2 * sizeof(Int64), _stream.size());
        }

        [TestMethod]
        public void Test_WriteUInt64() {
            _stream.write((UInt64)1);
            Assert.AreEqual<int>(1 * sizeof(UInt64), _stream.size());
            _stream.write((UInt64)2);
            Assert.AreEqual<int>(2 * sizeof(UInt64), _stream.size());
        }

        [TestMethod]
        public void Test_WriteSingle() {
            _stream.write((Single)1.1);
            Assert.AreEqual<int>(1 * sizeof(Single), _stream.size());
            _stream.write((Single)2.2);
            Assert.AreEqual<int>(2 * sizeof(Single), _stream.size());
        }

        [TestMethod]
        public void Test_WriteString() {
            _stream.write("1234567890");
            Assert.AreEqual<int>(10 + sizeof(Int16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteEmptyString() {
            _stream.write("");
            Assert.AreEqual<int>(sizeof(Int16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteArray() {
            int[] intArray = new int[10];
            for (int i = 0; i < 10; ++i) {
                intArray[i] = i;
            }
            intArray.serialize(_stream);
            Assert.AreEqual<int>(10 * sizeof(int) + sizeof(UInt16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteList() {
            List<int> intList = new List<int>();
            for (int i = 0; i < 10; ++i) {
                intList.Add(i);
            }
            intList.serialize(_stream);
            Assert.AreEqual<int>(10 * sizeof(int) + sizeof(UInt16), _stream.size());
        }

        [TestMethod]
        public void Test_WriteDictionary() {
            Dictionary<int, int> inDic = new Dictionary<int, int>();
            for (int i = 0; i < 10; ++i) {
                inDic.Add(i, i * 10);
            }
            inDic.serialize(_stream);
            Assert.AreEqual<int>(10 * sizeof(int) * 2 + sizeof(UInt16), _stream.size());
        }

        [TestMethod]
        public void Test_Reset() {
            _stream.write((Int32)1);
            Assert.AreEqual<int>(1 * sizeof(Int32), _stream.size());

            _stream.reset();
            Assert.AreEqual<int>(0, _stream.size());
        }
    }
}
