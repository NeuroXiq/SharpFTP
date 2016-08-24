using SharpFTP.Server.Connection;
using SharpFTP.Server.Protocol;
using SharpFTP.Server.Protocol.Commands;
using SharpFTP.Server.Protocol.Enums;
using System.Collections.Generic;
using System.Net.Sockets;


namespace SharpFTP.Server.CommandExecution
{
    abstract class CommandExecutionBase
    {
        protected TcpClient client;
        protected UserDataContext usersManage;
        protected ReplySender replySender;
        protected CommandReceive receiver;
        protected CommandParser commandParser;

        public CommandExecutionBase(TcpClient client)
        {
            this.client = client;
            receiver = new CommandReceive(client);
            replySender = new ReplySender(client);
            commandParser = new CommandParser();
            usersManage = ServerDataContext.Instance.UserDataContextProvider;
        }

        public abstract void ExecuteCommand(ClientCommand command);
        public abstract Command[] GetImplementedCommands();
    }
}
