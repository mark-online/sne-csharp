using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sne;

namespace SneCSharpUnitTest
{

    class MockRpcNetwork : RpcNetwork
    {
        public bool isForwarded { get; set; }
        public RpcId rpcId { get; set; }
        public int paramCount { get; set; }

        public override void forward(RpcId rpcId, params object[] args) {
            this.isForwarded = true;
            this.rpcId = rpcId;
            this.paramCount = args.Length;
        }
    }

    [RpcClass("TestRpc")]
    class TestRpcImpl : RpcController
    {
        public bool isReceiveCalled { get; set; }

        public TestRpcImpl(RpcNetwork rpcNetwork) :
            base(rpcNetwork) {}

        [RpcForward]
        public void rpcForward(bool a, int b) {
            forward(MethodBase.GetCurrentMethod().Name, a, b);
        }

        [RpcForward]
        public void rpcForward2() {
            forward(MethodBase.GetCurrentMethod().Name);
        }

        [RpcReceive]
        public void rpcReceive(InputStream inputStream) {
            isReceiveCalled = true;
        }

        [RpcReceive]
        public void rpcReceive2() {

        }
    }

    [TestClass]
    public class RpcAttributeTest
    {
        MockRpcNetwork _rpcNetwork = new MockRpcNetwork();
        TestRpcImpl _rpcImpl = null;

        [TestInitialize()]
        public void Initialize() {
            _rpcImpl = new TestRpcImpl(_rpcNetwork);
        }

        [TestMethod]
        public void Test_ForwardRpcCount() {
            Assert.AreEqual(2, _rpcImpl.forwardRpcCount);
        }

        [TestMethod]
        public void Test_Forward() {
            _rpcImpl.rpcForward(true, 0);
            Assert.IsTrue(_rpcNetwork.isForwarded);
            Assert.AreEqual(RpcController.makeRpcName("TestRpc", "rpcForward", 2),
                _rpcNetwork.rpcId.rpcName);
            Assert.AreEqual(2, _rpcNetwork.paramCount);
        }

        [TestMethod]
        public void Test_ReceiveRpcCount() {
            Assert.AreEqual(2, _rpcImpl.receiveRpcCount);
        }

        [TestMethod]
        public void Test_Receive() {
            StreamBuffer buffer = new StreamBuffer(100);
            InputStream inputStream = new InputStream(buffer);
            OutputStream outputStream = new OutputStream(buffer);
            outputStream.write((int)1);
            outputStream.write((short)2);
            outputStream.write("abcd");

            RpcId rpcId = new RpcId(RpcController.makeRpcName("TestRpc", "rpcReceive", 3));
            _rpcNetwork.handleRpc(rpcId.value, inputStream);
            Assert.AreEqual(2, _rpcImpl.receiveRpcCount);
        }
    }
}
