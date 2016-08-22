using SharpFTP.Server.CommandExecution;
using System.Net.Sockets;
using SharpFTP.Server.Protocol.Enums;
using System;
using SharpFTP.Server.Protocol.Commands;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class ConnectionEstablishment : CommandExecutionBase
    {
        public bool ConnectionEstablished { get; private set; } = false;
        

        public ConnectionEstablishment(TcpClient client) : base(client)
        { }

        public void EstablishConnection()
        {
            replySender.SendReply(220);
            ConnectionEstablished = true;
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            
        }

        public override Command[] GetImplementedCommands()
        {
            return null;
        }
    }
}
