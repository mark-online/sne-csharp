using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sne;

namespace SneCSharpUnitTest
{
    class DummyData : IStreamable
    {
        public string _s;
        public int _i;

        public void serialize(ByteStream stream) {
            if (stream.isInput()) {
                _s = (stream as InputStream).readString();
                _i = (stream as InputStream).readInt32();
            }
            else {
                (stream as OutputStream).write(_s);
                (stream as OutputStream).write(_i);
            }
        }
    }

    [TestClass]
    public class StreamableTest
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
        public void Test_Serialize() {
            DummyData output = new DummyData();
            output._s = "1234567890";
            output._i = 1234567890;
            output.serialize(_ostream);

            DummyData input = new DummyData();
            input.serialize(_istream);

            Assert.AreEqual(output._s, input._s);
            Assert.AreEqual(output._i, input._i);
        }
    }
}
