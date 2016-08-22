using SharpFTP.Server.Protocol;
using System.Net.Sockets;

namespace SharpFTP.Server.Connection
{
    public class ReplySender
    {
       // public bool AutoAddDescription { get; set; } = false;

        private NetworkStream stream;
        private ReplyBuilder replyBuilder;
        public ReplySender(TcpClient connectedClient)
        {
            stream = connectedClient.GetStream();
            this.stream = connectedClient.GetStream();
            this.replyBuilder = new ReplyBuilder();
        }

        //private object mutex = new object();

        public void SendReply(int replyCode)
        {
            byte[] message;
            
            message = replyBuilder.GetReplyBytesWithoutDescription(replyCode);
            
            SendRawReply(message);
        }

        public void SendSTATReply(string[] serverInformations)
        {
            byte[] bytes = replyBuilder.GetStatReplyBytes(serverInformations);
            //stream.Write(bytes, 0, bytes.Length);
            SendRawReply(bytes);
        }

        public void SendReply(int code, string customDescription)
        {
            byte[] bytes = replyBuilder.GetReplyBytes(code, customDescription);
            //stream.Write(bytes, 0, bytes.Length);
            SendRawReply(bytes);
        }

        public void SendRawReply(byte[] repeadBytes)
        {
           // lock (mutex)
           // {
                stream.Write(repeadBytes, 0, repeadBytes.Length);
                System.Console.ForegroundColor = System.ConsoleColor.Green;
                System.Console.WriteLine($"Sended reply => {System.Text.Encoding.ASCII.GetString(repeadBytes).TrimEnd('\r', '\n')}");
                System.Console.ForegroundColor = System.ConsoleColor.Gray;
           // }
        }
    }
}
