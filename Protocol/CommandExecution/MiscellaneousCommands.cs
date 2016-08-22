using SharpFTP.Server.CommandExecution;
using System;
using System.Net.Sockets;
using SharpFTP.Server.Protocol.Commands;
using SharpFTP.Server.Protocol.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;


namespace SharpFTP.Server.Protocol.CommandExecution
{
    class MiscellaneousCommands : CommandExecutionBase
    {
        public Command[] ImplementedCommands = new Command[] { NOOP, SITE };

        public MiscellaneousCommands(TcpClient client) : base(client)
        {

        }

        public override void ExecuteCommand(ClientCommand command)
        {
            switch (command.CommandType)
            {
                case NOOP:
                    ExecuteNoopCommand();
                    break;
                case SITE:
                    ExecuteSiteCommand();
                    break;
                default: throw new NotImplementedException("Command {command.CommandType} not implemented in this object.");
            }
        }

        private void ExecuteSiteCommand()
        {
            replySender.SendReply(202, "command not implemented at this site");
        }

        private void ExecuteNoopCommand()
        {
            replySender.SendReply(200, "command ok - noop reply");
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
