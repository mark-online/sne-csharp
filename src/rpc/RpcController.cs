using System;
using System.Reflection;
using System.Collections.Generic;

namespace sne
{

    public class RpcController
    {
        class RpcBind
        {
            RpcId _rpcId;
            MethodInfo _method;
            ParameterInfo[] _parameterInfos;
            object[] _parameters;

            public RpcId rpcId { get { return _rpcId; } }

            public RpcBind(RpcId rpcId, MethodInfo mi) {
                _rpcId = rpcId;
                _method = mi;
                _parameterInfos = mi.GetParameters();
            }

            public void execute(object instance, InputStream inputStream) {
                if (_parameters == null) {
                    _parameters = new object[_parameterInfos.Length];
                }
                for (int i = 0; i < _parameterInfos.Length; i++) {
                    ParameterInfo paramInfo = _parameterInfos[i];
                    Type paramType = paramInfo.ParameterType;
                    if (paramType == typeof(string)) {
                        _parameters[i] = string.Empty;
                    }
                    else if (paramType.BaseType == typeof(Array)) {
                        Type elementType = paramType.GetElementType();
                        _parameters[i] = Array.CreateInstance(elementType, 0);
                    }
                    else {
                        _parameters[i] = Activator.CreateInstance(paramType);
                    }
                    if (typeof(IStreamable).IsAssignableFrom(paramType)) {
                        (_parameters[i] as IStreamable).serialize(inputStream);
                    }
                    else {
                        inputStream.readObject(ref _parameters[i], paramType);
                    }
                }
                _method.Invoke(instance, _parameters);
            }
        }

        RpcNetwork _rpcNetwork;
        Dictionary<string, RpcId> _forwardRpcIdMap = new Dictionary<string, RpcId>();
        Dictionary<UInt32, RpcBind> _receiveRpcMap = new Dictionary<UInt32, RpcBind>();

        public int forwardRpcCount {
            get { return _forwardRpcIdMap.Count; }
        }

        public int receiveRpcCount {
            get { return _receiveRpcMap.Count; }
        }

        public RpcController(RpcNetwork rpcNetwork) {
            _rpcNetwork = rpcNetwork;
            _rpcNetwork.registerRpcController(this);

            initRpcs();
        }

        public void forward(string methodName, params object[] args) {
            _rpcNetwork.forward(getForwardRpcId(methodName), args);
        }

        public bool receive(out RpcId rpcId, UInt32 rpcIdValue, InputStream inputStream) {
            RpcBind rpcBind = getRpcBind(rpcIdValue);
            if (rpcBind == null) {
                rpcId = null;
                return false;
            }
            rpcId = rpcBind.rpcId;
            rpcBind.execute(this, inputStream);
            return true;
        }

        private RpcId getForwardRpcId(string methodName) {
            RpcId rpcId = null;
            _forwardRpcIdMap.TryGetValue(methodName, out rpcId);
            return rpcId;
        }

        private RpcBind getRpcBind(UInt32 rpcIdValue) {
            RpcBind rpcBind = null;
            _receiveRpcMap.TryGetValue(rpcIdValue, out rpcBind);
            return rpcBind;
        }

        private void initRpcs() {
            Type currentType = GetType();
            RpcClassAttribute classAttr =
                Attribute.GetCustomAttribute(currentType, typeof(RpcClassAttribute)) as RpcClassAttribute;
            if (classAttr == null) {
                throw new MessageException("RpcClassAttribute NOT FOUND!");
            }

            MethodInfo[] methods = currentType.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0, count = methods.Length; i < count; ++i) {
                MethodInfo mi = methods[i];
                if (Attribute.IsDefined(mi, typeof(RpcForwardAttribute))) {
                    string rpcName = makeRpcName(classAttr.className, mi.Name, mi.GetParameters().Length);
                    RpcId rpcId = new RpcId(rpcName);
                    _forwardRpcIdMap.Add(mi.Name, rpcId);
                }
                else if (Attribute.IsDefined(mi, typeof(RpcReceiveAttribute))) {
                    string rpcName = makeRpcName(classAttr.className, mi.Name, mi.GetParameters().Length);
                    RpcId rpcId = new RpcId(rpcName);
                    RpcBind rpcBind = new RpcBind(rpcId, mi);
                    _receiveRpcMap.Add(rpcId.value, rpcBind);
                }
            }
        }

        static public string makeRpcName(string className, string methodName, int paramCount) {
            return string.Format("{0}_{1}_{2}", className, methodName, paramCount);
        }
    }

} // namespace sne
