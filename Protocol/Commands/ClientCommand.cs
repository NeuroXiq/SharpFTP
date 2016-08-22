using SharpFTP.Server.Protocol.Enums;

namespace SharpFTP.Server.Protocol.Commands
{
    public class ClientCommand
    {
        public string Parameter { get; private set; } = string.Empty;
        public Command CommandType { get; private set; }
        public bool UnrecognizedCommand { get; private set; } = false;

        private ClientCommand()
        {
            this.UnrecognizedCommand = true;
        }

        public ClientCommand(Command type, string parameter)
        {
            this.CommandType = type;
            this.Parameter = parameter;
        }
        public ClientCommand(Command type)
        {
            this.CommandType = type;
        }

        public static ClientCommand GetUnrecognizedCommandInstance()
        {
            return new ClientCommand();
        }
    }
}
