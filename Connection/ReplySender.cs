using SharpFTP.Server.Protocol;
using System.Net.Sockets;

namespace SharpFTP.Server.Connection
{
    public class ReplySender
    {
        private NetworkStream stream;
        private ReplyBuilder replyBuilder;
        public ReplySender(TcpClient connectedClient)
        {
            stream = connectedClient.GetStream();
            this.stream = connectedClient.GetStream();
            this.replyBuilder = new ReplyBuilder();
        }

        public void SendReply(int replyCode)
        {
            byte[] message;
            
            message = replyBuilder.GetReplyBytesWithoutDescription(replyCode);
            
            SendRawReply(message);
        }

        public void SendSTATReply(string[] serverInformations)
        {
            byte[] bytes = replyBuilder.GetStatReplyBytes(serverInformations);
            SendRawReply(bytes);
        }

        public void SendReply(int code, string customDescription)
        {
            byte[] bytes = replyBuilder.GetReplyBytes(code, customDescription);
            SendRawReply(bytes);
        }

        public void SendRawReply(byte[] repeadBytes)
        {
            stream.Write(repeadBytes, 0, repeadBytes.Length);
            System.Console.ForegroundColor = System.ConsoleColor.Green;
            System.Console.WriteLine($"Sended reply => {System.Text.Encoding.ASCII.GetString(repeadBytes).TrimEnd('\r', '\n')}");
            System.Console.ForegroundColor = System.ConsoleColor.Gray;  
        }
    }
}
