using System;
using SharpFTP.Server.Protocol.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;
using System.Collections.Generic;
using SharpFTP.Server.Protocol.CommandExecution;

namespace SharpFTP.Server
{
    public static class FTPStaticServerState
    {
        public static  Command[] ImplementedCommands { get; private set; }
        public static CharType[] ImplementedCharTypes { get; private set; }
        public static PrintType[] ImplementedPrintTypes { get; private set; }
        public static string[] ServerInformations { get; private set; }
        public static string SystemType { get; private set; }

        static FTPStaticServerState()
        {
            ImplementedCommands = GetImplementedCommands();
            ImplementedCharTypes = GetImplementedCharTypes();
            ImplementedPrintTypes = GetImplementedPrintTypes();
            ServerInformations = GetServerInformations();
            SystemType = GetSystemType();
        }

        private static string GetSystemType()
        {
            return "UNIX Type: L8";
        }

        static string[] GetServerInformations()
        {
            string[] serverInfo = new string[]
            {
                "SharpFTP Server by NeuroXiq",
                "http://www.github.com/NeuroXiq",
                "Running on:" + Environment.OSVersion.ToString()
            };

            return serverInfo;
        }

        static  Command[] GetImplementedCommands()
        {
            List<Command> availableCommands = new List<Command>();

            availableCommands.AddRange(Login.ImplementedCommands);
            availableCommands.AddRange(Logout.ImplementedCommands);
            availableCommands.AddRange(TransferParameters.ImplementedCommands);
            availableCommands.AddRange(InformationalCommands.ImplementedCommands);
            availableCommands.AddRange(FileAction.ImplementedCommands);
            

            return availableCommands.ToArray();
        }

        static CharType[] GetImplementedCharTypes()
        {
            CharType[] implementedTypes = new CharType[]
            {
                CharType.ASCII
            };

            return implementedTypes;
        }

        static PrintType[] GetImplementedPrintTypes()
        {
            PrintType[] implementedPrints = new PrintType[]
            {
                PrintType.NonPrint
            };

            return implementedPrints;
        }
    }
}
