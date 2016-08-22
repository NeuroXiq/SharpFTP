using SharpFTP.Server.Connection;
using SharpFTP.Server.Protocol;
using SharpFTP.Server.Protocol.Enums;
using System.Net.Sockets;

namespace SharpFTP.Server
{
    class ClientSession
    {
        private CommandReceive receiver;
        private FTPProtocolInterpreter protocolInterpreter;
        private TcpClient acceptedClient;

        public ClientSession(TcpClient acceptedTcpClient)
        {
            acceptedClient = acceptedTcpClient; 
            this.receiver = new CommandReceive(acceptedTcpClient);
            this.protocolInterpreter = new FTPProtocolInterpreter(acceptedTcpClient);
        }

        public void RunSession()
        {
            protocolInterpreter.PrepareToNewClient();
            try
            {
                while (protocolInterpreter.ProcessNextCommand())
                {
                    if (!acceptedClient.Connected)
                        break;
                }
            }
            catch (System.Exception)
            {

            }
                        
        }
    }
}
