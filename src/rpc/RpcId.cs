using System;
using System.Collections.Generic;

namespace sne
{
    public class RpcId
    {
        public string rpcName;
        public UInt32 value;

        public RpcId(string name) {
            rpcName = name;
            value = hash(name);
        }

        private UInt32 hash(string str) {
            // CRC variant
            UInt32 h = 0;
            for (int i = 0; i < str.Length; ++i) {
                //byte ki = Convert.ToByte(str[i]);
                char ki = str[i];
                UInt32 highorder =(h & 0xf8000000);
                h <<= 5;
                h ^= (highorder >> 27);
                h ^= ki;
            }
            return h;
        }
    }

} // namespace sne
