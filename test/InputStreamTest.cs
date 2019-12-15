using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sne;

namespace SneCSharpUnitTest
{
    [TestClass]
    public class InputStreamTest
    {
        StreamBuffer _buffer = null;
        OutputStream _ostream = null;
        InputStream _istream = null;

        [TestInitialize()]
        public void Initialize() {
            _buffer = new StreamBuffer(10);
            _ostream = new OutputStream(_buffer);
            _istream = new InputStream(_buffer);
        }

        [TestMethod]
        public void Test_Empty() {
            Assert.AreEqual<int>(0, _istream.size());
        }

        [TestMethod]
        public void Test_ReadByte() {
            byte ovalue = 1;
            _ostream.write(ovalue);
            byte value = _istream.readByte();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<byte>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadSByte() {
            sbyte ovalue = 1;
            _ostream.write(ovalue);
            sbyte value = _istream.readSByte();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<sbyte>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadInt16() {
            Int16 ovalue = -1;
            _ostream.write(ovalue);
            Int16 value = _istream.readInt16();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<Int16>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadUInt16() {
            UInt16 ovalue = 65535;
            _ostream.write(ovalue);
            UInt16 value = _istream.readUInt16();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<UInt16>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadInt32() {
            Int32 ovalue = Int32.MinValue;
            _ostream.write(ovalue);
            Int32 value = _istream.readInt32();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<Int32>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadUInt32() {
            UInt32 ovalue = UInt32.MaxValue;
            _ostream.write(ovalue);
            UInt32 value = _istream.readUInt32();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<UInt32>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadInt64() {
            Int64 ovalue = Int64.MinValue;
            _ostream.write(ovalue);
            Int64 value = _istream.readInt64();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<Int64>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadUInt64() {
            UInt64 ovalue = UInt64.MaxValue;
            _ostream.write(ovalue);
            UInt64 value = _istream.readUInt64();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual<UInt64>(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadSingle() {
            float ovalue = -0.1234f;
            _ostream.write(ovalue);
            float value = _istream.readSingle();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual(ovalue, value, 0.001f);
        }

        [TestMethod]
        public void Test_ReadString() {
            string ovalue = "1234567890";
            _ostream.write(ovalue);
            string value = _istream.readString();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual(ovalue, value);
        }

        [TestMethod]
        public void Test_ReadArray() {
            int[] inArray = new int[10];
            for (int i = 0; i < 10; ++i) {
                inArray[i] = i;
            }
            inArray.serialize(_ostream);

            int[] outArray = new int[10];
            outArray.serialize(_istream);

            Assert.AreEqual<int>(0, _istream.size());
            for (int i = 0; i < 10; ++i) {
                Assert.AreEqual(inArray[i], outArray[i]);
            }
        }

        [TestMethod]
        public void Test_ReadList() {
            List<int> inList = new List<int>();
            for (int i = 0; i < 10; ++i) {
                inList.Add(i);
            }
            inList.serialize(_ostream);

            List<int> outList = new List<int>();
            outList.serialize(_istream);

            Assert.AreEqual<int>(0, _istream.size());
            for (int i = 0; i < 10; ++i) {
                Assert.AreEqual(inList[i], outList[i]);
            }
        }

        [TestMethod]
        public void Test_ReadDictionary() {
            Dictionary<int, int> inDic = new Dictionary<int, int>();
            for (int i = 0; i < 10; ++i) {
                inDic.Add(i, i * 10);
            }
            inDic.serialize(_ostream);

            Dictionary<int, int> outDic = new Dictionary<int, int>();
            outDic.serialize(_istream);

            Assert.AreEqual<int>(0, _istream.size());
            foreach (int key in inDic.Keys) {
                Assert.AreEqual(inDic[key], outDic[key]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(StreamException))]
        public void Test_Underflow() {
            _istream.readByte();
        }

        [TestMethod]
        public void Test_ReadEmtpyString() {
            string ovalue = "";
            _ostream.write(ovalue);
            string value = _istream.readString();
            Assert.AreEqual<int>(0, _istream.size());
            Assert.AreEqual(ovalue, value);
        }
    }
}
