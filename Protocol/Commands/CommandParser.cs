using System;
using System.Text;
using SharpFTP.Server.Protocol.Enums;
using SharpFTP.Server.Protocol;

namespace SharpFTP.Server.Protocol.Commands
{
    public class CommandParser
    {
        public enum  ParseResult
        {
            CommandOk,
            SyntaxError,
            ArgumentError,
            UnrecognizedCommand
        }

        public CommandParser()
        {

        }

        public ClientCommand ParseCommand(string commandString,out ParseResult result)
        {
            result = ParseResult.UnrecognizedCommand;
            ClientCommand parsedCommand = ClientCommand.GetUnrecognizedCommandInstance();
            commandString = commandString.TrimEnd('\r','\n').Trim(' ');
            string[] commandData = commandString.Split(' ');

            if (commandData.Length > 0)
            {
                string typeString = commandData[0].ToUpper();
                bool commandExist = Enum.IsDefined(typeof(Command), typeString);

                if (commandExist)
                {
                    Command cmdType = (Command)Enum.Parse(typeof(Command), typeString);
                    string param = GetParameters(commandString);

                    if (ValidateParameters(cmdType, param))
                    {
                        result = ParseResult.CommandOk;
                        parsedCommand = new ClientCommand(cmdType, param);
                    }
                    else
                    {
                        result = ParseResult.ArgumentError;
                        parsedCommand = ClientCommand.GetUnrecognizedCommandInstance();
                    }            
                }
            }

            return parsedCommand;
        }

        public ClientCommand ParseCommand(byte[] buffer, int lenght,out ParseResult result)
        {
            string cmd = Encoding.ASCII.GetString(buffer, 0, lenght);
            return ParseCommand(cmd,out result);
        }

        public Mode ParseMODEParameter(string parameter, out ParseResult result)
        {
            string param = parameter.Trim();
            if (string.IsNullOrWhiteSpace(parameter))
            {
                result = ParseResult.ArgumentError;
                return Mode.Block;
            }
            else
            {
                result = ParseResult.CommandOk;
                param = param.ToUpper();
                if (param == "S")
                    return Mode.Stream;
                else if (param == "B")
                    return Mode.Block;
                else if (param == "C")
                    return Mode.Compressed;
                else
                {
                    result = ParseResult.ArgumentError;
                    return Mode.Block;
                }
            }
        }

        public void ParseTYPEParameters(string parameters, out CharType charType, out PrintType printType)
        {
            parameters = parameters.Trim();
            string[] paramData = parameters.Split(' ');

            if (paramData.Length == 1)
            {
                string param = paramData[0].ToUpper();
                printType = PrintType.NonPrint;
                charType = ParseCharType(param);
            }
            else if (paramData.Length == 2)
            {
                printType = ParsePrintType(paramData[0].ToUpper());
                charType = ParseCharType(paramData[1].ToUpper());
            }
            else throw new Exception("Input parameter is not valid for TYPE command.");

        }

        private CharType ParseCharType(string param)
        {
            if (param == "I")
                return CharType.Image;
            else if (param == "E")
                return CharType.EBCDIC;
            else return CharType.ASCII;
        }

        private PrintType ParsePrintType(string param)
        {
            if (param == "N")
                return PrintType.NonPrint;
            else if (param == "T")
                return PrintType.TelnetFormatEffectors;
            else return PrintType.CarriageControl;
        }

        private string GetParameters(string command)
        {
            int spIndex = command.IndexOf(' ');
            int cutLenght = command.Length - spIndex;

            if (spIndex == -1)
                return string.Empty;

            string param = command.Substring(spIndex, cutLenght);

            return param.Trim();
        }

        private bool ValidateParameters(Command command, string parameters)
        {
            if (command == Command.TYPE)
            {
                return ValidateTYPEParameters(parameters);
            }
            else return true;
        }

        private bool ValidateTYPEParameters(string parameters)
        {
            return true;

            string[] parms = parameters.Split(' ');
            if (parms.Length == 0)
            {
                return false;
            }
            else if (parms.Length == 1)
            {
                string paramChar = parameters.ToUpper();
                if (paramChar == "I")
                    return true;
                else return false;
            }
            else if (parms.Length == 2)
            {
                if (parms[0] != "1" || parms[1] != "1")
                    return false;
                else
                {
                    char firstParam = parms[0][0];
                    char secParam = parms[0][1];

                    return
                        (firstParam == 'I' || firstParam == 'E' || firstParam == 'A') &&
                        (secParam == 'N' || secParam == 'T' || secParam == 'C');
                }
            }
            else return false;
        }

    }
}
