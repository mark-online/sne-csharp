using System;
using System.Collections.Generic;

namespace sne
{
    public abstract class ByteStream
    {
        public delegate void Serializer(ByteStream stream, ref object value);

        private Dictionary<Type, Serializer> _customSerializer;

        public virtual bool isInput() {
            return false;
        }
        public virtual bool isOutput() {
            return false;
        }

        public void registerSerializer(Type type, Serializer serializer) {
            if (_customSerializer == null) {
                _customSerializer = new Dictionary<Type, Serializer>();
            }
            _customSerializer.Add(type, serializer);
        }

        protected Serializer getSerializer(Type type) {
            if (_customSerializer == null) {
                return null;
            }
            Serializer serializer = null;
            _customSerializer.TryGetValue(type, out serializer);
            return serializer;
        }
    }

} // namespace sne
