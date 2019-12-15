using System;
using System.Collections.Generic;

namespace sne
{
    
    public class StreamBufferPool
    {
        const int bufferCapacity = 2048;
        const int listCapacity = 10;

        static List<StreamBuffer> _pool = new List<StreamBuffer>();
        static StreamBufferPool _instance = null;

        static public StreamBufferPool instance {
            get {
                if (_instance == null) {
                    _instance = new StreamBufferPool();
                }
                return _instance;
            }
        }

        public StreamBufferPool() {
            reserve(listCapacity);
        }

        public StreamBuffer acquire() {
            StreamBuffer buffer = null;
            lock (_pool) {
                if (_pool.Count <= 0) {
                    reserve(listCapacity / 2);
                }
                buffer = _pool[0];
                _pool.RemoveAt(0);
                buffer.reset();
            }
            return buffer;
        }

        public void release(StreamBuffer buffer) {
            lock (_pool) {
                _pool.Add(buffer);
            }
        }

        private void reserve(int capacity) {
            for (int i = 0; i < capacity; ++i) {
                _pool.Add(new StreamBuffer(bufferCapacity));
            }
        }
    }

} // namespace sne
