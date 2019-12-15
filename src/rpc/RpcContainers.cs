using System;
using System.Collections.Generic;
using System.Reflection;
using sne;

// 자동 로딩을 위해 namespace를 없앰

public static class StreamExtension
{

    public static Array serialize(this Array array, ByteStream stream) {
        if (stream.isInput()) {
            InputStream istream = stream as InputStream;
            int count = istream.readUInt16();
            if (count > 0) {
                Type elementType = array.GetType().GetElementType();
                if (array.Length != count) {
                    array = Array.CreateInstance(elementType, count);
                }
                for (int i = 0; i < count; ++i) {
                    object value = array.GetValue(i);
                    istream.readObject(ref value, elementType);
                    array.SetValue(value, i);
                }
            }
        }
        else {
            OutputStream ostream = stream as OutputStream;
            int count = array.Length;
            ostream.write((UInt16)count);
            if (array != null) {
                for (int i = 0; i < array.Length; ++i) {
                    ostream.writeObject(array.GetValue(i));
                }
            }
        }
        return array;
    }

    public static void serialize<T>(this List<T> list, ByteStream stream) {
        if (stream.isInput()) {
            InputStream istream = stream as InputStream;
            list.Clear();
            int count = istream.readUInt16();
            if (count > 0) {
                Type elementType = typeof(T);
                for (int i = 0; i < count; ++i) {
                    object value = Activator.CreateInstance(elementType);
                    istream.readObject(ref value, elementType);
                    list.Add((T)value);
                }
            }
        }
        else {
            OutputStream ostream = stream as OutputStream;
            int count = 0;
            if (list != null) {
                count = list.Count;
            }
            ostream.write((UInt16)count);
            for (int i = 0; i < count; ++i) {
                ostream.writeObject(list[i]);
            }
        }
    }

    public static void serialize<K, V>(this Dictionary<K, V> dic, ByteStream stream) {
        if (stream.isInput()) {
            InputStream istream = stream as InputStream;
            dic.Clear();
            int count = istream.readUInt16();
            if (count > 0) {
                Type keyType = typeof(K);
                Type valueType = typeof(V);
                for (int i = 0; i < count; ++i) {
                    object key = Activator.CreateInstance(keyType);
                    object value = Activator.CreateInstance(valueType);
                    istream.readObject(ref key, keyType);
                    istream.readObject(ref value, valueType);
                    dic.Add((K)key, (V)value);
                }
            }
        }
        else {
            OutputStream ostream = stream as OutputStream;
            int count = dic.Count;
            ostream.write((UInt16)count);
            if (count > 0) {
                var items = dic.GetEnumerator();
                while (items.MoveNext()) {
                    var key = items.Current.Key;
                    var value = items.Current.Value;
                    ostream.writeObject(key);
                    ostream.writeObject(value);
                }
            }
        }
    }

    public static void writeObject(this OutputStream ostream, IStreamable streamable) {
        streamable.serialize(ostream);
    }

    public static void readObject(this InputStream istream, ref IStreamable streamable, Type type) {
        streamable.serialize(istream);
    }
}
