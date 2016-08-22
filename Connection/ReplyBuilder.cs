using SharpFTP.Server.Protocol;
using System;
using System.Text;

namespace SharpFTP.Server.Connection
{
    public class ReplyBuilder
    {

        public ReplyBuilder()
        { }

        public byte[] GetReplyBytes(int replyCode, string replyMessage)
        {
            string code = Convert.ToString(replyCode);
            string fullMessageString = $"{replyCode} {replyMessage}\r\n";

            return Encoding.ASCII.GetBytes(fullMessageString);
        }

        internal byte[] GetReplyBytes(int replyCode)
        {
           return GetReplyBytes(replyCode,"");
        }

        public byte[] GetReplyBytesWithoutDescription(int replyCode)
        {
            string code = Convert.ToString(replyCode);
            return Encoding.ASCII.GetBytes($"{code}  \r\n");
        }

        public byte[] GetStatReplyBytes(string[] STATserverInformations)
        {
            string serverInfo = String.Join("\n", STATserverInformations);
            string startCode = "211- ";
            string endCode = "211 \r\n";

            string fullMessage = String.Join("", startCode, serverInfo, endCode);
            byte[] messageBytes = Encoding.ASCII.GetBytes(fullMessage);

            return messageBytes;
        }
    }
}
