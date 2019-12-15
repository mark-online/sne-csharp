using System;
using System.Reflection;
using sne;

namespace EchoClient
{

    [RpcClass("Echo")]
    class EchoRpcController : RpcController
    {
        public EchoRpcController(RpcNetwork rpcNetwork) :
            base(rpcNetwork) {}

        [RpcForward]
        public void echo(string msg) {
            forward(MethodBase.GetCurrentMethod().Name, msg);
        }

        [RpcReceive]
        private void onEcho(string msg) {
            echo(msg);
        }
    }

} // namespace EchoClient
