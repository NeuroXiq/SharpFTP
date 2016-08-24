using SharpFTP.Server.CommandExecution;
using SharpFTP.Server.FileSystem;
using SharpFTP.Server.Protocol.Commands;
using SharpFTP.Server.Protocol.Enums;
using System;
using System.Net.Sockets;
using static SharpFTP.Server.Protocol.Enums.Command;
using static SharpFTP.Server.UserDataContext;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class Login : CommandExecutionBase
    {
        public static Command[] ImplementedCommands { get; private set; } = new Command[] { USER, PASS, CWD };

        public bool IsLogged { get; private set; } = false;
        public string UserName { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public DirectorySession DirectorySession { get; private set; }

        private PathConverter pathConverter;

        public Login(TcpClient client) : base(client)
        {
            pathConverter = new PathConverter();
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            switch (command.CommandType)
            {
                case USER:
                    ExecuteUserCommand(command.Parameter);
                    break;
                case PASS:
                    ExecutePassCommand(command.Parameter);
                    break;
                case CWD:
                    ExecuteCwdCommand(command.Parameter);
                    break;
                default: throw new Exception("Unrecognized command");
            }
        }

        public void LogoutWithoutClosingConnection()
        {
            this.UserName = string.Empty;
            this.IsLogged = false;
            this.DirectorySession = null;
        }

        public UserInfo GetUserInfo()
        {
            return new UserInfo(UserName, Password);
        }

        private void ExecuteCwdCommand(string parameter)
        {
            if (IsLogged)
            {
                bool canChange = CanChangeDirectory(parameter);

                if (canChange)
                {
                    replySender.SendReply(250, "working directory changed - successful");
                    this.DirectorySession.ChangeWorkingDirectory(parameter);
                }
                else
                {
                    replySender.SendReply(550, "received path is file not a directory");
                }
            }
            else
            {
                replySender.SendReply(530, "Not logged in.");
            }
        }

        private void ExecutePassCommand(string parameter)
        {
            IsLogged = false;
            if (userContext.RequirePassword(UserName))
            {
                if (userContext.PasswordCorrect(UserName, parameter))
                {
                    replySender.SendReply(230, "user logged in successful");
                    IsLogged = true;
                    DirectorySession = new DirectorySession(directoryContext.GetOriginDirectory(UserName));
                }
                else
                {
                    replySender.SendReply(530, "not logged in");
                }
            }
            else replySender.SendReply(503, "bad sequenc of command (password not required)");
        }

        private bool CanChangeDirectory(string unixPath)
        {
            string winPath = string.Empty;
            bool canChange = false;

            try
            {
                winPath = pathConverter.ConvertToWindowsPath(
                unixPath, directoryContext.GetOriginDirectory(UserName));

                canChange = directoryContext.CanChangeDirectory(winPath, UserName);
            }
            catch (Exception e)
            {
                canChange = false;
            }

            return canChange;
        }

        /// <summary>
        /// Validating login process.
        /// </summary>
        /// <param name="userName">ASCII representation of received USER command parameter.</param>
        public void ExecuteUserCommand(string userName)
        {
            int replyCode = -1;
            string message = "";
            IsLogged = false;

            if (userContext.UserExist(userName))
            {
                if (userContext.RequirePassword(userName))
                {
                    replyCode = 331;//password require
                    message = "password require";
                }
                else
                {
                    replySender.SendReply(230, "user logged in successful");
                    IsLogged = true;
                    DirectorySession = new DirectorySession(directoryContext.GetOriginDirectory(UserName));
                }
            }
            else //user does not exist in database.
            {
                replyCode = 530;
                message = "user does not exist. not logged in.";
            }
            replySender.SendReply(replyCode,message);
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
