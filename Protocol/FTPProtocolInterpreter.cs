using System;
using System.Net.Sockets;
using System.Linq;
using SharpFTP.Server.Protocol.Enums;
using System.Text;
using static SharpFTP.Server.Protocol.Enums.Command;
using SharpFTP.Server.FileSystem;
using SharpFTP.Server.CommandExecution;
using SharpFTP.Server.Connection;
using SharpFTP.Server.Protocol.CommandExecution;
using SharpFTP.Server.Protocol.Commands;

namespace SharpFTP.Server.Protocol
{
    public class FTPProtocolInterpreter  
    {
        private Login login;
        private Logout logout;
        private TransferParameters transferParameters;
        private FileAction fileAction;
        private ReplySender replySender;
        private CommandReceive receiver;
        private TcpClient client;
        private InformationalCommands informationalCommands;

        public FTPProtocolInterpreter(TcpClient connectedClient)
        {
            receiver = new CommandReceive(connectedClient);
            login = new Login(connectedClient);
            replySender = new ReplySender(connectedClient);
            client = connectedClient;
            transferParameters = new TransferParameters(client);
            logout = new Logout(connectedClient,transferParameters,login);
            this.informationalCommands = new InformationalCommands(connectedClient);
        }

        /// <summary>
        /// Receiving command from connected client and executing it.
        /// </summary> 
        /// <returns> If protocol interpreter can execute new command returns true. </returns>
        public bool ProcessNextCommand()
        {
            CommandParser.ParseResult result;
            ClientCommand command = receiver.ReceiveCommand(out result);
            if (result == CommandParser.ParseResult.CommandOk)
                HandleCommand(command);
            else SendParsingErrorReply(result);
            if (command.CommandType == Command.QUIT)
                return false;
            else return true;
        }

        public void HandleCommand(ClientCommand command)
        {
            if (Login.ImplementedCommands.Contains(command.CommandType))
                ExecuteLoginCommand(command);
            else if (Logout.ImplementedCommands.Contains(command.CommandType))
                ExecuteLogoutCommand(command);
            else if (FileAction.ImplementedCommands.Contains(command.CommandType))
                ExecuteFileActionCommand(command);
            else if (TransferParameters.ImplementedCommands.Contains(command.CommandType))
                ExecuteTransferParametersCommand(command);
            else if (InformationalCommands.ImplementedCommands.Contains(command.CommandType))
                ExecuteInformationalCommand(command);
            else replySender.SendReply(500, "command not implemented");
            
        }

        private void ExecuteInformationalCommand(ClientCommand command)
        {
            informationalCommands.ExecuteCommand(command);
        }

        public void PrepareToNewClient()
        {
            replySender.SendReply(220);
        }

        private void ExecuteTransferParametersCommand(ClientCommand command)
        {
            if (transferParameters == null)
                transferParameters = new TransferParameters(client);
            transferParameters.ExecuteCommand(command);
        }

        private void ExecuteFileActionCommand(ClientCommand command)
        {
            if (fileAction == null)
                fileAction = new FileAction(client, transferParameters, login);
            fileAction.ExecuteCommand(command);
        }

        private void ExecuteLogoutCommand(ClientCommand command)
        {
            logout.ExecuteCommand(command);
        }

        private void ExecuteLoginCommand(ClientCommand command)
        {
            login.ExecuteCommand(command);
        }

        private void SendParsingErrorReply(CommandParser.ParseResult result)
        {
            switch (result)
            {
                case CommandParser.ParseResult.SyntaxError:
                    replySender.SendReply(500);
                    break;
                case CommandParser.ParseResult.ArgumentError:
                    replySender.SendReply(501);
                    break;
                case CommandParser.ParseResult.UnrecognizedCommand:
                    replySender.SendReply(500);
                    break;
                default:
                    break;
            }
        }
    }
}
