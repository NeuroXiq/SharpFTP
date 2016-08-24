using SharpFTP.Server.CommandExecution;
using System;
using System.Net.Sockets;
using SharpFTP.Server.Protocol.Enums;
using SharpFTP.Server.FileSystem.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;
using System.Net;
using SharpFTP.Server.FileSystem;
using System.IO;
using System.Threading;
using System.Text;
using SharpFTP.Server.Protocol.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class FileAction : CommandExecutionBase
    {
        public static Command[] ImplementedCommands { get; private set; }
            = new Command[] { ALLO, REST, STOR, RETR, LIST, APPE, RNFR, RNTO, DELE, PWD, ABOR, SIZE, MKD };

        public int MemoryToAllocate { get; private set; } = 0;
        public int RestStartPosition { get; private set; } = 0;
        public bool TransferingInProgress { get; private set; } = false;

        private TransferParameters transferParameters;
        private PathConverter pathConverter;
        private Login login;
        private string renameFrom;
        private bool renameFromAccepted = false;

        public FileAction(TcpClient client,TransferParameters transferParameters,Login login) : base(client)
        {
            this.pathConverter = new PathConverter();
            this.login = login;
            this.transferParameters = transferParameters;
            
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            MemoryToAllocate = 0;
            switch (command.CommandType)
            {
                case ALLO:
                    ExecuteAlloCommand(command.Parameter);
                    break;
                case REST:
                    ExecuteRestCommand(command.Parameter);
                    break;
                case STOR:
                    ExecuteStorCommand(command.Parameter);
                    break;
                case RETR: ExecuteRetrCommand(command.Parameter);
                    break;
                case LIST:
                    ExecuteListCommand(command.Parameter);
                    break;
                case APPE: ExecuteStorCommand(command.Parameter); //same as store
                    break;
                case RNFR:
                    ExecuteRnfrCommand(command.Parameter);
                    break;
                case RNTO:
                    ExecuteRntoCommand(command.Parameter);
                    break;
                case SIZE:
                    ExecuteSizeCommand(command.Parameter);
                    break;
                case DELE:
                    ExecuteDeleCommand(command.Parameter);
                    break;
                case MKD:
                    ExecuteMkdCommand(command.Parameter);
                    break;
                case PWD:
                    ExecutePwdCommand();
                    break;
                case ABOR:
                    ExecuteAborCommand();   
                    break;
                default: throw new Exception($"Command not implemented {command.CommandType} with param {command.Parameter}");
            }
        }

        private void ExecuteRntoCommand(string parameter)
        {
            if (login.IsLogged)
            {
                string winSourceDir = pathConverter.ConvertToWindowsPath(renameFrom, directoryContext.GetOriginDirectory(login.UserName));
                string winDestDir = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));
                if (renameFromAccepted)
                {
                    bool success = directoryContext.RenamePath(winSourceDir,winDestDir);
                    if (success)
                    {
                        replySender.SendReply(250, "renamed success");
                    }
                    else
                    {
                        replySender.SendReply(553, "request not takien - name not allowed");
                    }
                }
                else
                {
                    replySender.SendReply(503, "bad sequence of command - first send rnfr");
                }
            }
            else
            {
                replySender.SendReply(530, "not logged in");
            }

            renameFromAccepted = false;
        }

        private void ExecuteRnfrCommand(string parameter)
        {
            if (login.IsLogged)
            {
                string winDir = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));
                if (directoryContext.CanRename(winDir, login.UserName))
                {
                    this.renameFrom = parameter;
                    this.renameFromAccepted = true;
                    replySender.SendReply(350, "start renaming file");
                }
                else
                {
                    replySender.SendReply(450, "cannot change directory");
                }
            }
            else
            {
                replySender.SendReply(530, "not logged in ");
            }
        }

        private void ExecuteAborCommand()
        {
            transferParameters.DataTransfer.AbortTransfer();
            replySender.SendReply(226, "data task aborted.");
        }

        private void ExecuteDeleCommand(string parameter)
        {
            if (login.IsLogged)
            {
                string winPath = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));
                if (directoryContext.CanDelete(winPath, login.UserName))
                {
                    directoryContext.DeletePath(winPath);
                    replySender.SendReply(250, "file deleted successful");
                }
                else replySender.SendReply(450, "action not taken");
                
            }
            else replySender.SendReply(530, "not logged in");
        }

        private void ExecuteStorCommand(string parameter)
        {
            if (login.IsLogged)
            {
                if (directoryContext.CanCreateDirectory(parameter, login.UserName))
                {
                    string winPath = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));

                    if (transferParameters.DataTransfer.TransferInProgress)
                    {
                        transferParameters.DataTransfer.AbortTransfer();
                        replySender.SendReply(426, "aborted last transfering");
                    }
                    replySender.SendReply(150, "data transfer opened - start downloading file");
                    FileStream fileStream = new FileStream(winPath, FileMode.OpenOrCreate);
                    transferParameters
                        .DataTransfer
                        .RunReceiveBytesTask
                        (fileStream, 1000, 
                        () => DataTransferSuccessful("received file successful"));
                }
                else
                {
                    replySender.SendReply(450, "requested action not taken");
                }
            }
            else
            {
                replySender.SendReply(530, "not logged in");
            }
        }

        private void ExecuteMkdCommand(string parameter)
        {
            if (login.IsLogged)
            {
                //257 "pathname" created
                if (directoryContext.CanCreateDirectory(parameter, login.UserName))
                {
                    directoryContext.CreateDirectory(parameter);
                    replySender.SendReply(257, $"\"{parameter}\" created");
                }
                else
                {
                    replySender.SendReply(550, "cannot create directory.");
                }
            }
            else
            {
                replySender.SendReply(530, "cannot create directory - not logged in");
            }
        }

        private void ExecuteRetrCommand(string parameter)
        {
            string winPath = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));

            if ((directoryContext.GetFilePermission(winPath, login.UserName) & FilePermission.Read) == FilePermission.Read)
            {
                Stream fileStream = directoryContext.GetFileStream(winPath);

                if (transferParameters.DataTransfer.TransferInProgress)
                {
                    transferParameters.DataTransfer.TransferDataTaskCancell.Cancel();
                    replySender.SendReply(426, "aborting last file transfer.");
                }
                replySender.SendReply(150, "transfer socket open - start sending");
                transferParameters.DataTransfer.RunSendStreamTask(
                    fileStream,
                    (bytesTransmitted) =>
                     { DataTransferSuccessful("data transfere succesfull"); fileStream.Close(); });
            }
            else
            {
                replySender.SendReply(550, "no access");
            }
        }

        private void ExecutePwdCommand()
        {
            string workingDir = string.Format("\"{0}\"", login.DirectorySession.WorkingUnixDirectory);
            replySender.SendReply(257, workingDir);
        }

        private void ExecuteSizeCommand(string parameter)
        {
            replySender.SendReply(213, "12345");
        }

        private void ExecuteListCommand(string parameter)
        {
            parameter = parameter.Trim();

            bool permission = directoryContext.CanChangeDirectory(login.DirectorySession.WorkingWindowsDirectory, login.UserName);

            if(permission)
            {                
                string[] files = login.DirectorySession.GetFilesNamesInUnixFormat();
                string[] directories = login.DirectorySession.GetDirectoriesInUnixFormat();
                string[] paths = files.Concat(directories).ToArray();
                string singleLineText = string.Join("\r\n", paths);
                int a = singleLineText.Length;
                if (transferParameters.DataTransfer.TransferInProgress)
                {
                    transferParameters.DataTransfer.TransferDataTaskCancell.Cancel();
                    replySender.SendReply(426, "aborting last file transfer.");
                }
                replySender.SendReply(150, "transfer socket open - start sending");

                transferParameters.DataTransfer.RunSendTextDataTask(
                    singleLineText,
                    (bytesTransmitted) => 
                    {
                        DataTransferSuccessful(
                        $"sended file list");
                    });
            }
            else
            {
                replySender.SendReply(450, "file action not taken");
            }
        }

        private void DataTransferSuccessful(string message)
        {
            transferParameters.DataTransfer.CloseConnection();
            replySender.SendReply(226, message);
        }

        private void ExecuteRestCommand(string parameter)
        {
            int restParsed;
            bool parsingResutl = int.TryParse(parameter, out restParsed);
            if (parsingResutl)
            {
                replySender.SendReply(350, "rest setter successfull");
                RestStartPosition = restParsed;
            }
            else
            {
                throw new Exception("incorect arg");
            }
        }

        private void ExecuteAlloCommand(string parameter)
        {
            //200,202
            string[] data = parameter.Split(' ');
            int memoryToAllocate;

            if (int.TryParse(data[1], out memoryToAllocate))
            {
                MemoryToAllocate = memoryToAllocate;
                replySender.SendReply(200);
            }
            else
            {
                throw new Exception("incorrect parameter");
            }
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
