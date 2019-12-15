using System;
using System.Collections.Generic;

namespace sne
{
    public abstract class RpcNetwork
    {
        public delegate void RpcForwardingDelegate(RpcId rpcId);
        public RpcForwardingDelegate delRpcForwarding;

        public delegate void RpcReceivingDelegate(RpcId rpcId);
        public RpcReceivingDelegate delRpcReceiving;

        List<RpcController> _rpcControllers = new List<RpcController>();

        public void registerRpcController(RpcController rpcController) {
            _rpcControllers.Add(rpcController);
        }

        public bool handleRpc(UInt32 rpcIdValue, InputStream inputStream) {
            RpcId rpcId;
            for (int i = 0, count = _rpcControllers.Count; i < count; ++i) {
                RpcController rpcController = _rpcControllers[i];
                if (rpcController.receive(out rpcId, rpcIdValue, inputStream)) {
                    if (delRpcReceiving != null) {
                        delRpcReceiving(rpcId);
                    }
                    return true;
                }
            }
            return false;
        }

        public virtual void forward(RpcId rpcId, params object[] args) {
            if (delRpcForwarding != null) {
                delRpcForwarding(rpcId);
            }
        }
    }
} // namespace sne
