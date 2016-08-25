using SharpFTP.Server.Protocol.Enums;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using SharpFTP.Server.Protocol.CommandExecution;

namespace SharpFTP.Server.DataTransfer
{
    class FTPDataTransfer
    {
        public CancellationTokenSource TransferDataTaskCancell { get; private set; }
        public int LastTransferBytesCount { get; private set; }
        public bool TransferInProgress { get; private set; }
        public delegate void TaskEndDelegate(int bytesTransmited);

        private static object threadSafe = new object();
        private TcpClient connectedClient;
        private NetworkStream networkStream;
        private Task transferTask;
        private TransferParameters transferParameters;
        private bool isPassive;

        private FTPDataTransfer(TcpClient connectedClient,TransferParameters parameters,bool isPassive)
        {
            this.connectedClient = connectedClient;
            this.networkStream = connectedClient.GetStream();
            this.transferParameters = parameters;
            this.isPassive = isPassive;
            TransferDataTaskCancell = new CancellationTokenSource();
        }

        public static FTPDataTransfer AcceptPasvConnection(TcpListener listener,TransferParameters parameters)
        {
            lock (threadSafe)
            {
                TcpClient client;
                client = listener.AcceptTcpClient();
                listener.Stop();

                return new FTPDataTransfer(client,parameters,true);
            }
        }

        public static FTPDataTransfer ConnectToPort(IPAddress ipAddress,int port,TransferParameters parameters)
        {
            lock (threadSafe)
            {
                TcpClient client = new TcpClient();
                client.Connect(new IPEndPoint(ipAddress, port));

                return new FTPDataTransfer(client,parameters,false);
            }
        }

        public void SendTextData(string text)
        {
            ThrowIfParamsIncorrect();

            byte[] messageBytes = Encoding.ASCII.GetBytes(text);
            networkStream.Write(messageBytes, 0, messageBytes.Length);
        }

        /// <summary>
        /// Send raw bytes over data transfer.
        /// </summary>
        /// <param name="bytes">
        /// Bytes to send
        /// </param>
        /// <param name="lenght">
        /// Bytes count to send starting from first index of bytes array.
        /// </param>
        public void SendRawBytes(byte[] bytes,int lenght)
        {       
            networkStream.Write(bytes, 0, lenght);
        }

        /// <summary>
        /// Writing received bytes in to passed stream
        /// </summary>
        /// <param name="timeout">
        /// Maximum wait time for new packet in miliseconds
        /// </param>
        public void RunReceiveBytesTask(Stream receiveStream,int timeout, TaskEndDelegate taskCompleteSuccessful,TaskEndDelegate taskCancelled)
        {
            ThrowIfParamsIncorrect();
            if (TransferInProgress)
                throw new InvalidOperationException("Cannot run second transfer task until previos is not completed");

            transferTask = Task.Factory.StartNew(() =>
            {
                int readedAll = ReceiveBytesFromClient(receiveStream, timeout);

                if(isPassive)
                {
                    int port = (connectedClient.Client.LocalEndPoint as IPEndPoint).Port;
                    FTPDynamicServerState.RelasePort(port);
                }

                if (!TransferDataTaskCancell.Token.IsCancellationRequested)
                    taskCompleteSuccessful?.Invoke(readedAll);
                else taskCompleteSuccessful?.Invoke(readedAll);
            });
        }

        private int ReceiveBytesFromClient(Stream receiveStream, int timeout)
        {
            TransferInProgress = false;

            int readedAll = 0;
            {
                int bufferSize = 64 * 64;
                int readed = 0;

                byte[] buffer = new byte[bufferSize];

                do
                {
                    readedAll += (readed = networkStream.Read(buffer, 0, bufferSize));
                    if (readed == 0)
                    {
                        Thread.Sleep(timeout);
                        readedAll += (readed = networkStream.Read(buffer, 0, bufferSize));
                        if (readed == 0)
                            break;
                    }

                    receiveStream.Write(buffer, 0, readed);
                    buffer = new byte[bufferSize];

                } while (!TransferDataTaskCancell.Token.IsCancellationRequested);

                LastTransferBytesCount = readedAll;

            }
            TransferInProgress = false;
            return readedAll;
        }

        public void RunSendTextDataTask(string text,TaskEndDelegate taskCompletedSuccessful,TaskEndDelegate taskCancelled)
        {
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
            RunSendStreamTask(stream, taskCompletedSuccessful,taskCancelled);
        }
        
        public void RunSendStreamTask(Stream stream, TaskEndDelegate taskCompletedSuccessful,TaskEndDelegate taskCancelled)
        {
            ThrowIfParamsIncorrect();
            if (TransferInProgress)
                throw new InvalidOperationException("Cannot run second transfer task until previos is not completed");

            transferTask = Task.Factory.StartNew(() =>
            {
                int allReadedBytes = SendSteamToClient(stream);

                if (isPassive)
                {
                    int port = (connectedClient.Client.LocalEndPoint as IPEndPoint).Port;
                    FTPDynamicServerState.RelasePort(port);
                }

                if (!TransferDataTaskCancell.Token.IsCancellationRequested)
                    taskCompletedSuccessful?.Invoke(allReadedBytes);
                else taskCancelled?.Invoke(allReadedBytes);

            }, TransferDataTaskCancell.Token);
            
        }

        private int SendSteamToClient(Stream stream)
        {
            int allReadedBytes = 0;

            lock (threadSafe)
            {
                TransferInProgress = true;
            }
            if (stream.Length == 0)
            {
                LastTransferBytesCount = 0;
            }
            else
            {
                TransferDataTaskCancell = new CancellationTokenSource();
                int bufferSize = 64 * 64;
                int readedBytes = 0;


                byte[] buffer = new byte[bufferSize];

                do
                {
                    allReadedBytes += (readedBytes = stream.Read(buffer, 0, bufferSize));
                    networkStream.Write(buffer, 0, readedBytes);
                    buffer = new byte[bufferSize];

                } while (readedBytes == bufferSize && !TransferDataTaskCancell.Token.IsCancellationRequested);
                LastTransferBytesCount = allReadedBytes;
            }

            lock (threadSafe)
            {
                TransferInProgress = false;
            }

            return allReadedBytes;
        }

        public void AbortTransfer()
        {
            if (TransferInProgress)
            {
                TransferDataTaskCancell.Cancel();
                //maybe request is sended after check for cancellation. 
                //must wait for next loop.
                transferTask.Wait();
            }
        }

        public void CloseConnection()
        {
            try
            {
                connectedClient.GetStream().Flush();
                AbortTransfer();
                connectedClient.Close();
                connectedClient.Dispose();
            }
            catch (Exception)
            {
                //client can be disconnected - everythink is ok.
            }
        }

        private void ThrowIfParamsIncorrect()
        {
            if (transferParameters.Mode != Mode.Stream ||
                (transferParameters.CharMode != CharType.Image &&
                transferParameters.CharMode != CharType.ASCII))
                throw new NotImplementedException("Not implemented char type or mode. Implemented types is ASCII and Stream");
        }
    }
}
