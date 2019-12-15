using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace sne
{

    struct MessageCallback
    {
        public Type _messageClassType;
        public Action<Message> _callback;
    }

    public class ClientSession<OStream, IStream> : RpcNetwork, IDisposable
        where OStream : OutputStream, new()
        where IStream : InputStream, new()
    {
        const int connectionTimeout = 5000;

        enum State
        {
            NotConnected,
            Connecting,
            Connected,
            Validated
        }

        public delegate void DisconnectedDelegate();
        public DisconnectedDelegate delDisconnected;

        public delegate void ErrorDelegate(string message);
        public ErrorDelegate delError;

        State _state = State.NotConnected;
        Socket _socket;

        Queue<StreamBuffer> _sendQueue = new Queue<StreamBuffer>();
        Queue<StreamBuffer> _receiveQueue = new Queue<StreamBuffer>();

        StreamBuffer _recvBuffer;
        StreamBuffer _marshalBuffer;

        OStream _sendStream;
        OStream _marshalStream;
        IStream _unmarshalStream;

        PacketHeader _receivedHeader;

        float _heartbeatInterval; // seconds
        DateTime _lastSentTime;
        HeartbeatMessage _heartbeatMsg;

        Dictionary<MessageType.Value, MessageCallback> _messageCallbackMap =
            new Dictionary<MessageType.Value, MessageCallback>();

        bool _alreadyDisposed = false;

        #region Properties

        public bool isConnected {
            get { return _state >= State.Connected; }
        }

        public ulong bandwidthIn { get; private set; }
        public ulong bandwidthOut { get; private set; }

        public int sendQueueSize { get { lock (_sendQueue) { return _sendQueue.Count; } } }
        public int receiveQueueSize { get { lock (_receiveQueue) { return _receiveQueue.Count; } } }

        public int sendPacketCount { get; private set; }
        public int receivePacketCount { get; private set; }

        #endregion Properties

        public ClientSession(float heartbeatInterval) {
            _heartbeatInterval = heartbeatInterval;

            _recvBuffer = StreamBufferPool.instance.acquire();
            _marshalBuffer = StreamBufferPool.instance.acquire();

            _sendStream = new OStream();
            _marshalStream = new OStream();
            _marshalStream.replace(_marshalBuffer);
            _unmarshalStream = new IStream();

            _receivedHeader = new PacketHeader(_recvBuffer);

            registerMessageCallback(
                MessageType.Value.csmtSystem_heartbeat,
                heartbeatCallback, typeof(HeartbeatMessage));
        }

        public void registerSerializer(Type type, ByteStream.Serializer serializer) {
            _marshalStream.registerSerializer(type, serializer);
            _unmarshalStream.registerSerializer(type, serializer);
        }

        public bool connect(string host, UInt16 port) {
            if (isConnected) {
                disconnect();
            }

            _state = State.Connecting;
            string errorMessage = string.Empty;
            var mrEvent = new System.Threading.ManualResetEvent(false);
            try {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                for (int i = 0; i < addresses.Length; ++i) {
                    try {
                        var address = addresses[i];
                        if ((address.AddressFamily != AddressFamily.InterNetwork) &&
                            (address.AddressFamily != AddressFamily.InterNetworkV6)) {
                            continue;
                        }

                        mrEvent.Reset();

                        var endPoint = new IPEndPoint(address, port);
                        createSocket(address.AddressFamily);
                        IAsyncResult result = _socket.BeginConnect(endPoint, (res) => {
                            mrEvent.Set();
                        }, null);
                        bool isActive = mrEvent.WaitOne(connectionTimeout);
                        if (isActive) {
                            _socket.EndConnect(result);
                            connected();
                            return true;
                        }
                        errorMessage = "Connection timeout!";
                    }
                    catch (SocketException e) {
                        if (i >= (addresses.Length - 1)) {
                            errorMessage = e.Message;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(errorMessage)) {
                    errorMessage = "Address not available";
                }
            }
            catch (Exception e) {
                errorMessage = e.Message;
            }

            error(errorMessage);
            close();
            return false;
        }

        public void disconnect() {
            close();
        }

        public void sendMessage(Message msg) {
            StreamBuffer marshaledBuffer = marshal(msg);

            ushort bodySize = (ushort)marshaledBuffer.size();
            byte messageType = (byte)msg.getMessageType();

            _sendStream.replace(StreamBufferPool.instance.acquire());
            _sendStream.write(bodySize);
            _sendStream.write(messageType);
            if (bodySize > 0) {
                _sendStream.write(marshaledBuffer.data, bodySize);
            }
            send(_sendStream.buffer);
        }

        public void registerMessageCallback(MessageType.Value mt, Action<Message> callback,
            Type messageClassType) {
            MessageCallback msgCallback = new MessageCallback();
            msgCallback._messageClassType = messageClassType;
            msgCallback._callback = callback;
            _messageCallbackMap.Add(mt, msgCallback);
        }

        public void handleMessages() {
            if (!isConnected) {
                return;
            }

            while (true) {
                StreamBuffer buffer = getReceivedMessage();
                if (buffer == null) {
                    break;
                }

                try {
                    if (!handleMessage(buffer)) {
                        break;
                    }
                }
                finally {
                    StreamBufferPool.instance.release(buffer);
                }
            }

            sendHeartbeatMessage();
        }

        public void validated() {
            _state = State.Validated;
            _lastSentTime = DateTime.Now;
        }

        private void sendHeartbeatMessage() {
            if (_state < State.Validated) {
                return;
            }
            if (_heartbeatInterval <= 0) {
                return;
            }

            float elapsed = (float)((DateTime.Now - _lastSentTime).TotalMilliseconds / 1000);
            if (elapsed > _heartbeatInterval) {
                if (_heartbeatMsg == null) {
                    _heartbeatMsg = new HeartbeatMessage();
                }
                sendMessage(_heartbeatMsg);
            }
        }

        private void send(StreamBuffer buffer) {
            if (!isConnected) {
                StreamBufferPool.instance.release(buffer);
                return;
            }

            lock (_sendQueue) {
                _sendQueue.Enqueue(buffer);

                if (_sendQueue.Count == 1) {
                    doSend(buffer);
                }
            }

            _lastSentTime = DateTime.Now;
            ++sendPacketCount;
        }

        private void close() {
            _state = State.NotConnected;

            try {
                if ((_socket != null) && _socket.Connected) {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }

                Dispose();
            }
            catch (Exception) {}

            reset();
        }

        private void createSocket(AddressFamily addressFamily) {
            if (_socket != null) {
                try {
                    _socket.Close();
                }
                catch {}
            }
            _socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true; // disable Nagle algorithm
        }

        private void beginReceive() {
            // Queue up the read operation
            try {
                doReceive();
            }
            catch (Exception ex) {
                error(ex.Message);
                disconnected();
            }
        }

        private void doSend(StreamBuffer buffer) {
            try {
                _socket.BeginSend(buffer.data, buffer.readIndex, buffer.size(),
                    SocketFlags.None, onSend, null);
            }
            catch (Exception ex) {
                error(ex.Message);
                disconnected();
            }
        }

        private void doReceive() {
            _socket.BeginReceive(_recvBuffer.data, _recvBuffer.writeIndex, _recvBuffer.space(),
                SocketFlags.None, onReceive, null);
        }

        private void error(string message) {
            if (delError != null) {
                delError(message);
            }
        }

        private bool handleMessage(StreamBuffer buffer) {
            bool isHandled = false;

            _unmarshalStream.replace(buffer);
            MessageType.Value mt = (MessageType.Value)_unmarshalStream.readByte();
            if (mt == MessageType.Value.csmtRpc) {
                UInt32 rpcIdValue = _unmarshalStream.readUInt32();
                isHandled = handleRpc(rpcIdValue, _unmarshalStream);
                if (!isHandled) {
                    throw new MessageException(string.Format("RPC({0}) not found", rpcIdValue));
                }
            }
            else {
                MessageCallback msgCallback;
                if (_messageCallbackMap.TryGetValue(mt, out msgCallback)) {
                    Message message = MessageCache.query(msgCallback._messageClassType);
                    message.serialize(_unmarshalStream);
                    msgCallback._callback(message);

                    isHandled = true;
                }
                else {
                    throw new MessageException(string.Format("Message({0}) not found", mt.ToString()));
                }
            }
            ++receivePacketCount;
            return isHandled;
        }

        private StreamBuffer getReceivedMessage() {
            lock (_receiveQueue) {
                if (_receiveQueue.Count > 0) {
                    return _receiveQueue.Dequeue();
                }
            }
            return null;
        }

        private StreamBuffer marshal(RpcId rpcId, params object[] args) {
            _marshalStream.reset();
            _marshalStream.write(rpcId.value);
            for (int i = 0, count = args.Length; i < count; ++i) {
                object arg = args[i];
                if (arg is IStreamable) {
                    (arg as IStreamable).serialize(_marshalStream);
                }
                else {
                    _marshalStream.writeObject(arg);
                }
            }
            return _marshalStream.buffer;
        }

        private StreamBuffer marshal(Message msg) {
            _marshalStream.reset();
            msg.serialize(_marshalStream);
            return _marshalStream.buffer;
        }

        private void reset() {
            // recycle buffers
            var sendBuffers = _sendQueue.GetEnumerator();
            while (sendBuffers.MoveNext()) {
                StreamBuffer buffer = sendBuffers.Current;
                StreamBufferPool.instance.release(buffer);
            }
            _sendQueue.Clear();

            var recvBuffers = _receiveQueue.GetEnumerator();
            while (recvBuffers.MoveNext()) {
                StreamBuffer buffer = recvBuffers.Current;
                StreamBufferPool.instance.release(buffer);
            }
            _receiveQueue.Clear();

            _recvBuffer.reset();

            _receivedHeader.reset();

            bandwidthIn = 0;
            bandwidthOut = 0;

            sendPacketCount = 0;
            receivePacketCount = 0;
        }

        private void connected() {
            _state = State.Connected;

            beginReceive();
        }

        private void disconnected() {
            close();

            if (delDisconnected != null) {
                delDisconnected();
            }
        }

        private bool packetReceived() {
            while (_receivedHeader.isPacketHeaderArrived()) {
                if (!_receivedHeader.parse()) {
                    return false;
                }
                if (!_receivedHeader.isMessageArrived()) {
                    break;
                }

                messageReceived();
                _receivedHeader.reset();
            }
            return true;
        }

        private void messageReceived() {
            StreamBuffer buffer = StreamBufferPool.instance.acquire();
            buffer.write((byte)_receivedHeader.messageType);
            if (_receivedHeader.bodySize > 0) {
                buffer.copyFrom(_recvBuffer.data, _recvBuffer.readIndex, _receivedHeader.bodySize);
                _recvBuffer.readIndex += _receivedHeader.bodySize;
                _recvBuffer.crunch();
            }

            lock (_receiveQueue) {
                _receiveQueue.Enqueue(buffer);
            }
        }

        #region Message callback

        private void heartbeatCallback(sne.Message msg) {
            // NOP
        }

        #endregion // Message callback

        #region RpcNetwork overriding

        public override void forward(RpcId rpcId, params object[] args) {
            base.forward(rpcId, args);

            StreamBuffer marshaledBuffer = marshal(rpcId, args);

            ushort bodySize = (ushort)marshaledBuffer.size();
            byte messageType = (byte)MessageType.Value.csmtRpc;

            _sendStream.replace(StreamBufferPool.instance.acquire());
            _sendStream.write(bodySize);
            _sendStream.write(messageType);
            _sendStream.write(marshaledBuffer.data, marshaledBuffer.size());
            send(_sendStream.buffer);
        }

        #endregion // RpcNetwork overriding

        #region Socket callback

        void onSend(IAsyncResult result) {
            if (!isConnected) {
                return;
            }

            int bytes = 0;
            try {
                bytes = _socket.EndSend(result);
            }
            catch (Exception ex) {
                error(ex.Message);
                disconnected();
                return;
            }
            bandwidthOut += (ulong)bytes;

            StreamBuffer usedBuffer = null;
            lock (_sendQueue) {
                usedBuffer = _sendQueue.Dequeue();

                if (bytes > 0) {
                    // If there is another packet to send out, let's send it
                    if (_sendQueue.Count > 0) {
                        StreamBuffer nextBuffer = _sendQueue.Peek();
                        doSend(nextBuffer);
                    }
                }
                else {
                    disconnected();
                }
            }

            if (usedBuffer != null) {
                // The buffer has been sent and can now be safely recycled
                StreamBufferPool.instance.release(usedBuffer);
            }
        }

        void onReceive(IAsyncResult result) {
            if (!isConnected) {
                return;
            }

            int bytes = 0;
            try {
                bytes = _socket.EndReceive(result);
            }
            catch (Exception ex) {
                error(ex.Message);
                disconnected();
                return;
            }

            if (bytes == 0) {
                disconnected();
                return;
            }
            bandwidthIn += (uint)bytes;

            _recvBuffer.writeIndex += bytes;

            if (!packetReceived()) {
                disconnected();
                return;
            }

            if (!isConnected) {
                return;
            }

            beginReceive();
        }

        #endregion // Socket callback

        #region IDisposable implement

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        #endregion

        protected virtual void Dispose(bool isDisposing) {
            if (_alreadyDisposed) {
                return;
            }
            if (isDisposing) {
                _socket.Close();
            }
            _alreadyDisposed = true;
        }
    }

} // namespace sne
