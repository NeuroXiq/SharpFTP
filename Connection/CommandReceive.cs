using SharpFTP.Server.Protocol;
using SharpFTP.Server.Protocol.Commands;
using System;
using System.Net.Sockets;
using System.Text;

namespace SharpFTP.Server.Connection
{
    public class CommandReceive
    {
        private TcpClient client;
        CommandParser commandParser;

        public CommandReceive(TcpClient client)
        {
            this.client = client;
            this.commandParser = new CommandParser();
        }

        public ClientCommand ReceiveCommand(out CommandParser.ParseResult parsingResult)
        {
            byte[] buffer = new byte[128];
            NetworkStream stream = client.GetStream();
            int readedBytes = 0;
            while (!CRLFReached(buffer, readedBytes))
            {
                if (stream.CanRead)
                {
                    readedBytes += stream.Read(buffer, readedBytes, 1);
                }
            }
            string wrr = Encoding.ASCII.GetString(buffer).TrimEnd('\r', '\n','\0');
            Console.WriteLine(wrr);
            
            ClientCommand parsedCommand = commandParser.ParseCommand(buffer,readedBytes,out parsingResult);
            return parsedCommand;
        }

        private bool CRLFReached(byte[] buffer, int dataLenght)
        {
            if (dataLenght > 1)
            {
                return buffer[dataLenght - 2] == '\r' &&
                    buffer[dataLenght - 1] == '\n';
            }
            else return false;
        }
    }
}
