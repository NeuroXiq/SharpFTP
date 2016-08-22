using System;
using System.Net.Sockets;
using SharpFTP.Server.CommandExecution;
using SharpFTP.Server.Protocol.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;
using SharpFTP.Server.Protocol.Commands;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class Logout : CommandExecutionBase
    {
        private TransferParameters transfer;
        private Login login;

        public static Command[] ImplementedCommands { get; private set; } = new Command[] { REIN, QUIT };

        public Logout(TcpClient client, TransferParameters transfer,Login login) : base(client)
        {
            this.transfer = transfer;
            this.login = login;
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            switch (command.CommandType)
            {
                case REIN:
                    ExecuteReinCommand();
                    break;
                case QUIT:
                    replySender.SendReply(221, "Closing telnet connection");
                    break;
                default: throw new Exception("Unrecognized commands");
            }
        }

        private void ExecuteReinCommand()
        {
            if (transfer != null)
                transfer.DataTransfer.CloseConnection();
            login.LogoutWithoutClosingConnection();
            replySender.SendReply(220, "service ready for new user");
            
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
