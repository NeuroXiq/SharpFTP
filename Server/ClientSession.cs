using SharpFTP.Server.Connection;
using SharpFTP.Server.Protocol;
using SharpFTP.Server.Protocol.Enums;
using System;
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
            bool connected = protocolInterpreter.PrepareToNewClient();
            try
            {
                if (connected)
                {
                    
                    while (protocolInterpreter.ProcessNextCommand())
                    {
                        if (!acceptedClient.Connected)
                            break;
                    }
                }
            }
            catch (System.Exception e)
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Magenta;

                Console.WriteLine("********************* GLOBAL EXCEPTION ********************");
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------");

                Console.ForegroundColor = ConsoleColor.Gray;
#endif
            }

            
                        
        }
    }
}
