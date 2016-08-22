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
        protected IDataContextProvider dataContext;
        protected IDirectoryProvider directoryContext;
        protected IUserProvider userContext;
        protected ReplySender replySender;
        protected CommandReceive receiver;
        protected CommandParser commandParser;

        public CommandExecutionBase(TcpClient client)
        {
            this.client = client;
            dataContext = ServerDataContext.Instance.DataContextInstance;
            directoryContext = dataContext.DirectoryProvider;
            userContext = dataContext.UserProvider;
            receiver = new CommandReceive(client);
            replySender = new ReplySender(client);
            commandParser = new CommandParser();

            
        }

        public abstract void ExecuteCommand(ClientCommand command);
        public abstract Command[] GetImplementedCommands();
    }
}
