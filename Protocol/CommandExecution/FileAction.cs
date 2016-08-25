using SharpFTP.Server.CommandExecution;
using System;
using System.Net.Sockets;
using SharpFTP.Server.Protocol.Enums;
using SharpFTP.Server.FileSystem.Enums;
using static SharpFTP.Server.Protocol.Enums.Command;
using SharpFTP.Server.FileSystem;
using System.IO;
using SharpFTP.Server.Protocol.Commands;
using System.Linq;
using static SharpFTP.Server.UserDataContext;
using SharpFTP.Server.DataTransfer;

namespace SharpFTP.Server.Protocol.CommandExecution
{
    class FileAction : CommandExecutionBase
    {
        public static Command[] ImplementedCommands { get; private set; }
            = new Command[] { ALLO, REST, STOR, RETR, LIST, APPE, RNFR, RNTO, DELE, PWD, ABOR, SIZE, MKD };

        public int MemoryToAllocate { get; private set; } = 0;
        public int RestStartPosition { get; private set; } = 0;
        public bool TransferingInProgress { get; private set; } = false;

        private struct RenameCmdInfo
        {
            public string RenameFrom;
            public string RenameTo;
            public bool RnfrReceived;

            public void Reset()
            {
                RenameFrom = string.Empty;
                RenameTo = string.Empty;
                RnfrReceived = false;
            }
        }

        private TransferParameters transferParameters;
        private PathConverter pathConverter;
        private Login login;
        private RenameCmdInfo renameInfo;

        public FileAction(TcpClient client,TransferParameters transferParameters,Login login) : base(client)
        {
            this.pathConverter = new PathConverter();
            this.login = login;
            this.transferParameters = transferParameters;
            
        }

        public override void ExecuteCommand(ClientCommand command)
        {
            if (!login.IsLogged)
            {
                replySender.SendReply(530, "not logged in");
                return;
            }

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
            //if (login.IsLogged)
            //{
            //    string winSourceDir = pathConverter.ConvertToWindowsPath(renameFrom, directoryContext.GetOriginDirectory(login.UserName));
            //    string winDestDir = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));
            //    if (renameFromAccepted)
            //    {
            //        bool success = directoryContext.RenamePath(winSourceDir,winDestDir);
            //        if (success)
            //        {
            //            replySender.SendReply(250, "renamed success");
            //        }
            //        else
            //        {
            //            replySender.SendReply(553, "request not takien - name not allowed");
            //        }
            //    }
            //    else
            //    {
            //        replySender.SendReply(503, "bad sequence of command - first send rnfr");
            //    }
            //}
            //else
            //{
            //    replySender.SendReply(530, "not logged in");
            //}
            //
            //renameFromAccepted = false;
        }

        private void ExecuteRnfrCommand(string parameter)
        {
            //if (login.IsLogged)
            //{
            //    string winDir = pathConverter.ConvertToWindowsPath(parameter, directoryContext.GetOriginDirectory(login.UserName));
            //    if (directoryContext.CanRename(winDir, login.UserName))
            //    {
            //        this.renameFrom = parameter;
            //        this.renameFromAccepted = true;
            //        replySender.SendReply(350, "start renaming file");
            //    }
            //    else
            //    {
            //        replySender.SendReply(450, "cannot change directory");
            //    }
            //}
            //else
            //{
            //    replySender.SendReply(530, "not logged in ");
            //}
        }

        private void ExecuteAborCommand()
        {
            transferParameters.DataTransfer.AbortTransfer();
            replySender.SendReply(226, "data task aborted.");
        }

        private void ExecuteDeleCommand(string parameter)
        {
            string winPath = GetWindowsPath(parameter);
            bool haveAccess = usersManage.HaveAccess(login.GetUserInfo(), winPath);

            if (haveAccess)
            {
                FilePermission permission = usersManage.GetPathPermission(login.GetUserInfo(), winPath);
                if ((permission & FilePermission.Write) == FilePermission.Write)
                {
                    bool mutexOn = FileMutex.MutexOn(winPath);
                    if (mutexOn)
                    {
                        bool deleted = TryDeleteFile(winPath);
                        if (deleted)
                        {
                            replySender.SendReply(250, "file deleted successful");
                        }
                        else
                        {
                            replySender.SendReply(450, "exception throw during deleting file");
                        }
                        FileMutex.MutexOff(winPath);
                    }
                    else
                    {
                        replySender.SendReply(450, "file unavailable, busy");
                    }
                }
                else
                {
                    replySender.SendReply(550, "permission error, cannot edit file");
                }
            }
        }

        private bool TryDeleteFile(string winPath)
        {
            bool success = false;
            try
            {
                File.Delete(winPath);
                success = true;
            }
            catch (Exception)
            {
                
            }

            return success;
        }

        private void ExecuteStorCommand(string parameter)
        {
            string winPath = GetWindowsPath(parameter);
            bool mutexOn = FileMutex.MutexOn(winPath);
            UserInfo user = login.GetUserInfo();

            if (mutexOn)
            {
                if (usersManage.HaveAccess(user, winPath))
                {
                    if ((usersManage.GetPathPermission(user, winPath) & FilePermission.Write) == FilePermission.Write)
                    {
                        if (File.Exists(winPath))
                        {
                            File.Delete(winPath);
                        }
                        //ok - can receive file bytes
                        replySender.SendReply(150, "data transfer opened - start downloading file");
                        ReceiveFile(winPath);
                    }
                    else replySender.SendReply(450, "invalid file permission");
                }
                else
                {
                    replySender.SendReply(450, "access denied");
                }
            }
            else replySender.SendReply(450, "store command rejected - file busy");
        }

        private void ReceiveFile(string savePath)
        {
            FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate);

            FTPDataTransfer.TaskEndDelegate receivingSuccessful  = delegate (int receivedBytes)
            {
                transferParameters.DataTransfer.CloseConnection();
                string msg = $"received all - closing connection ({receivedBytes} bytes)";
                replySender.SendReply(226, msg);
                fs.Close();
                FileMutex.MutexOff(savePath);
            };

            FTPDataTransfer.TaskEndDelegate cancell = delegate (int receivedBytes)
            {
                fs.Close();
                FileMutex.MutexOff(savePath);
            };

            transferParameters.DataTransfer.RunReceiveBytesTask(fs, 500, receivingSuccessful,cancell);
        }

        private void ExecuteMkdCommand(string parameter)
        {
            string winPath = GetWindowsPath(parameter);
            bool canAccess = usersManage.HaveAccess(login.GetUserInfo(), winPath);

            if (!Directory.Exists(winPath))
            {
                Directory.CreateDirectory(winPath);
                replySender.SendReply(257, "directory created successful");
            }
            else
            {
                replySender.SendReply(550, "MKD refuse - directory already exist");
            }
        }

        private void ExecuteRetrCommand(string parameter)
        {
            string fileName = pathConverter.ConvertToWindowsFileName(parameter, login.DirectorySession.OriginDirectory);

            if (usersManage.HaveAccess(login.GetUserInfo(), fileName) && File.Exists(fileName))
            {
                FilePermission permission = usersManage.GetPathPermission(login.GetUserInfo(), fileName);
                if ((permission & FilePermission.Read) == FilePermission.Read)
                {
                    if (FileMutex.MutexOn(fileName))
                    {
                        SendFileBytes(fileName);
                    }
                    else
                    {
                        replySender.SendReply(450, "retr not taken - file busy (mutex on)");
                    }
                }
                else replySender.SendReply(550, "access denied - need read permission");

            }
            else replySender.SendReply(550, "retr not taken");
        }

        private void SendFileBytes(string winPath)
        {
            FileStream fileStream = new FileStream(winPath, FileMode.Open);
            FTPDataTransfer.TaskEndDelegate successDelegate 
                =  delegate (int bytesSended)
            {
                transferParameters.DataTransfer.CloseConnection();
                replySender.SendReply(226, $"bytes received successful ({bytesSended}) - closing data transfer");
                fileStream.Close();
                FileMutex.MutexOff(winPath);
            };
            FTPDataTransfer.TaskEndDelegate cancellDelegate = delegate (int bytesSended) 
            {
                FileMutex.MutexOff(winPath);
                fileStream.Close();
            };
            

            replySender.SendReply(150, "opening data connection");
            transferParameters.DataTransfer.RunSendStreamTask(fileStream,successDelegate,cancellDelegate);
        }

        private void ExecutePwdCommand()
        {
            string workingDir = string.Format("\"{0}\"", login.DirectorySession.WorkingUnixDirectory);
            replySender.SendReply(257, workingDir);
        }

        private void ExecuteSizeCommand(string parameter)
        {
            replySender.SendReply(213, "1");
        }

        private void ExecuteListCommand(string parameter)
        {
            string winPath = login.DirectorySession.WorkingWindowsDirectory;//GetWindowsPath(login.DirectorySession.WorkingUnixDirectory);
            bool permission = usersManage.HaveAccess(login.GetUserInfo(), winPath);
            bool exist = Directory.Exists(winPath);

            if(permission && exist)
            {
                SendFileList(winPath);
            }
            else
            {
                string msg = "file action not taken";
                if (!exist)
                    msg = string.Format("{0} '{1}'", msg, "directory not exist");
                if (!permission)
                    msg = string.Format("{} '{1}'", msg, "no access");


                int code = exist ? 450 : 530;

                replySender.SendReply(code, msg);
            }
        }

        private void SendFileList(string winPath)
        {
            string fileList = GetFileListString(winPath);

            if (transferParameters.DataTransfer.TransferInProgress)
            {
                transferParameters.DataTransfer.TransferDataTaskCancell.Cancel();
                replySender.SendReply(426, "aborting last file transfer.");
            }

            replySender.SendReply(150, "transfer socket opened - start sending");

            FTPDataTransfer.TaskEndDelegate successful = delegate (int sendedBytes)
            {
                transferParameters.DataTransfer.CloseConnection();
                replySender.SendReply(226, $"closing data connection - sended file list ({sendedBytes} Bytes)");
            };
            FTPDataTransfer.TaskEndDelegate cancell = delegate (int sendedBytes) {/* no action*/ };

            transferParameters.DataTransfer.RunSendTextDataTask(fileList, successful, cancell);
        }

        private string GetFileListString(string winPath)
        {
            string[] files = login.DirectorySession.GetFilesNamesInUnixFormat();
            string[] directories = login.DirectorySession.GetDirectoriesInUnixFormat();
            string[] paths = files.Concat(directories).ToArray();
            string singleLineText = string.Join("\r\n", paths);

            return singleLineText;
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

        private string GetWindowsPath(string unixPath)
        {
            string unix;
            try
            {
                unix = pathConverter.ConvertToWindowsDirectory(unixPath, login.DirectorySession.OriginDirectory);
            }
            catch (Exception)
            {
                unix = string.Empty;
            }

            return unix;
        }

        public override Command[] GetImplementedCommands()
        {
            return ImplementedCommands;
        }
    }
}
