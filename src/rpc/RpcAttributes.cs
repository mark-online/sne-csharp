using System;

namespace sne
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RpcClassAttribute : Attribute
    {
        string _className;

        public string className { get { return _className; } }

        public RpcClassAttribute(string className) {
            _className = className;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RpcForwardAttribute : Attribute
    {
        public RpcForwardAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RpcReceiveAttribute : Attribute
    {
        public RpcReceiveAttribute() { }
    }
}
