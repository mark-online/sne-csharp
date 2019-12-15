using System;

namespace EchoClient
{
    class Runner
    {
        bool useRpc = false;

        sne.ClientSession<sne.OutputStream, sne.InputStream> session;
        EchoRpcController echoRpcController;

        public Runner() {
            session = new sne.ClientSession<sne.OutputStream, sne.InputStream>(5);
            session.delDisconnected += disconnected;
            session.delError += errorOccurred;

            if (useRpc) {
                echoRpcController = new EchoRpcController(session);
            }
            else {
                session.registerMessageCallback(
                    sne.MessageType.toMessageType((int)EchoMessageType.emtEcho),
                    echoCallback,
                    typeof(EchoMessage));
            }
        }

        public void run() {
            if (!session.connect("localhost", 34567)) {
                return;
            }

            sendEcho("1234567890");

            while (session.isConnected) {
                session.handleMessages();
            }
        }

        private void sendEcho(string text) {
            if (useRpc) {
                echoRpcController.echo(text);
            }
            else {
                EchoMessage msg = new EchoMessage();
                msg.text = text;
                session.sendMessage(msg);
            }
        }

        private void echoCallback(sne.Message msg) {
            EchoMessage echoMessage = msg as EchoMessage;
            sendEcho(echoMessage.text);
        }

        private void disconnected() {
            Console.WriteLine("Disconnected!");
        }

        private void errorOccurred(string msg) {
            Console.WriteLine("Error: " + msg);
        }
    }

    class Program
    {
        static void Main(string[] args) {
            Runner runner = new Runner();
            runner.run();

            Console.WriteLine("Bye bye~");
        }
    }
}
