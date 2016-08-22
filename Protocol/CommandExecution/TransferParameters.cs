using SharpFTP.Server.CommandExecution;
using System;
using System.Net.Sockets;
using SharpFTP.Server.Protocol.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;
using System.Net;
using SharpFTP.Server.DataTransfer;
using SharpFTP.Server.Protocol.Commands;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class TransferParameters : CommandExecutionBase
    {
        public static Command[] ImplementedCommands { get; private set; } = new Command[] { PORT, PASV, MODE, TYPE, STRU };

        public IPAddress IpAddress { get; private set; } = IPAddress.None;
        public FTPDataTransfer DataTransfer { get; private set; }

        public Mode Mode { get; private set; } = Mode.Stream;
        public CharType CharMode { get; private set; } = CharType.ASCII;
        public PrintType PrintMode { get; private set; } = PrintType.NonPrint;
        public Structure StructureMode { get; private set; } = Structure.File;

        public int ClientPort { get; private set; } = default(int);
        public int PasvLocalPort { get; private set; } = default(int);
        public bool IsPassiveMode { get; private set; } = false;

        public TransferParameters(TcpClient client) : base(client)
        {
        }
        /*
         port -> server to client (client send gist port)
         pasv -> client to server// server send: 227 =h1,h2,h3,h4,p1,p2
         */
        public override void ExecuteCommand(ClientCommand command)
        {
            switch (command.CommandType)
            {
                case PORT:
                    ExecutePortCommand(command.Parameter);
                    break;
                case PASV:
                    ExecutePasvCommand();
                    break;
                case MODE:
                    ExecuteModeCommand(command.Parameter);
                    break;
                case TYPE:
                    ExecuteTypeCommand(command.Parameter);
                    break;
                case STRU:
                    ExecuteStruCommand(command.Parameter);
                    break;
                default: throw new Exception($"command not implemented {command.CommandType} with parameter {command.Parameter}");
            }
        }

        private void ExecuteStruCommand(string parameter)
        {
            parameter = parameter.Trim().ToUpper();
            if (parameter == "F")
                StructureMode = Structure.File;
            else if (parameter == "R")
                StructureMode = Structure.Record;
            else if (parameter == "P")
                StructureMode = Structure.Page;
            else throw new Exception("Stru parameter incorrect");

            if (StructureMode != Structure.File)
            {
                replySender.SendReply(504, $"stru command not implemented for {StructureMode.ToString()}");
                StructureMode = Structure.File;
            }
            else StructureMode = Structure.File;

        }

        private void ExecuteTypeCommand(string parameter)
        {
            string[] data = parameter.ToUpper().Trim().Split(' ');

            if (data[0] == "A")
                CharMode = CharType.ASCII;
            else if (data[0] == "E")
                CharMode = CharType.EBCDIC;
            else if (data[0] == "I")
                CharMode = CharType.Image;
            else throw new Exception("undefined parameter");

            PrintMode = PrintType.NonPrint;

            if (data.Length == 2)
            {
                if (data[0] == "A")
                    CharMode = CharType.ASCII;
                else if (data[0] == "E")
                    CharMode = CharType.EBCDIC;
                else if (data[0] == "I")
                    CharMode = CharType.Image;
                else throw new Exception("undefined parameter");

                if (data[1] == "N")
                    PrintMode = PrintType.NonPrint;
                else if (data[1] == "T")
                    PrintMode = PrintType.TelnetFormatEffectors;
                else if (data[1] == "C")
                    PrintMode = PrintType.CarriageControl;
                else throw new Exception("undefined second parameter");
            }

            if (CharMode == CharType.EBCDIC || PrintMode != PrintType.NonPrint)
            {
                replySender.SendReply(504, "command not implemented for this parameter");
                CharMode = CharType.Image;
                PrintMode = PrintType.NonPrint;
            }            
            else
            replySender.SendReply(200,"command ok");
        }

        private void ExecuteModeCommand(string parameter)
        {
            parameter = parameter.Trim().ToUpper();

            if (parameter == "S")
                Mode = Mode.Stream;
            else if (parameter == "B")
                Mode = Mode.Block;
            else if (parameter == "C")
                Mode = Mode.Compressed;
            else throw new Exception($"Undefined parameter: {parameter}");
        }

        private void ExecutePasvCommand()
        {
            int port = FTPDynamicServerState.GetPasvPort();
            string portString = $"{(int)(port / 256)},{port % 256}";
            string ipString = FTPDynamicServerState.GetIp().ToString().Replace('.', ',');
            string reply = $"Entering Passive Mode. (192,168,1,2,{portString})";
            
            //accepting pasv connection
            string s = $"{227} {reply}\r\n";

            if (DataTransfer != null)
                DataTransfer.CloseConnection();
            AbortTransferIsExist();
            replySender.SendRawReply(System.Text.Encoding.ASCII.GetBytes(s));
            DataTransfer = FTPDataTransfer.AcceptPasvConnection(port,this);
        }

        private void AbortTransferIsExist()
        {
            if (DataTransfer != null)
            {
                if (DataTransfer.TransferInProgress)
                {
                    DataTransfer.AbortTransfer();
                    DataTransfer.CloseConnection();
                    replySender.SendReply(426, "data transfer aborted - opening new process");
                }
            }
        }

        private void ExecutePortCommand(string parameter)
        {
            IsPassiveMode = false;

            string param = parameter.Trim();
            string[] data = param.Split(',');

            IPAddress ip = IPAddress.Parse($"{data[0]}.{data[1]}.{data[2]}.{data[3]}");
            int port = (int.Parse(data[4]) * 256) + int.Parse(data[5]);
            replySender.SendReply(200, "command ok");
            AbortTransferIsExist();
            DataTransfer = FTPDataTransfer.ConnectToPort(ip, port,this);
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
