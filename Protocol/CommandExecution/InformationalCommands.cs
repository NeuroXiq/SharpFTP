using SharpFTP.Server.CommandExecution;
using SharpFTP.Server.Protocol.Commands;
using SharpFTP.Server.Protocol.Enums;
using System;
using System.Net.Sockets;
using static SharpFTP.Server.Protocol.Enums.Command;



namespace SharpFTP.Server.Protocol.CommandExecution
{
    class InformationalCommands : CommandExecutionBase
    {
        public static Command[] ImplementedCommands { get; private set; } = new Command[] { SYST, STAT, };

        private TcpClient client;

        public InformationalCommands(TcpClient client):base(client)
        {
            this.client = client;
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            switch (command.CommandType)
            {
                case SYST:
                    ExecuteSystCommand();
                    break;
                case STAT:
                    ExecuteStatCommand();
                    break;
                default:
                    throw new Exception($"command not implemented: {command.CommandType} with param {command.Parameter}");
            }
        }

        private void ExecuteStatCommand()
        {
            string[] statInfo = FTPStaticServerState.ServerInformations;
            replySender.SendSTATReply(statInfo);
        }

        private void ExecuteSystCommand()
        {
            replySender.SendReply(215, FTPStaticServerState.SystemType);
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
