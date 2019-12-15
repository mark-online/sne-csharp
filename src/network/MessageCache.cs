using System;
using System.Collections.Generic;

namespace sne
{
    static public class MessageCache
    {
        static Dictionary<Type, Message> messageMap = new Dictionary<Type, Message>();

        static public Message query(Type type) {
            Message message = null;
            if (!messageMap.TryGetValue(type, out message)) {
                message = Activator.CreateInstance(type) as Message;
                messageMap.Add(type, message);
            }
            return message;
        }
    }
}
