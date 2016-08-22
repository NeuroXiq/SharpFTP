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
        public delegate void TaskCompleteSuccesfull(int bytesTransmited);

        private static object threadSafe = new object();
        private TcpClient connectedClient;
        private NetworkStream networkStream;
        private Task transferTask;
        private TransferParameters transferParameters;

        private FTPDataTransfer(TcpClient connectedClient,TransferParameters parameters)
        {
            this.connectedClient = connectedClient;
            this.networkStream = connectedClient.GetStream();
            this.transferParameters = parameters;
            TransferDataTaskCancell = new CancellationTokenSource();
        }

        public static FTPDataTransfer AcceptPasvConnection(int port,TransferParameters parameters)
        {
            lock (threadSafe)
            {
                TcpClient client;
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                client = listener.AcceptTcpClient();
                listener.Stop();

                return new FTPDataTransfer(client,parameters);
            }
        }

        public static FTPDataTransfer ConnectToPort(IPAddress ipAddress,int port,TransferParameters parameters)
        {
            lock (threadSafe)
            {
                TcpClient client = new TcpClient();
                client.Connect(new IPEndPoint(ipAddress, port));

                return new FTPDataTransfer(client,parameters);
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
        /// <param name="receivedStream">
        /// Stream which bytes are written
        /// </param>
        /// <param name="timeout">
        /// Maximum wait time for new packet in miliseconds
        /// </param>
        public void RunReceiveBytesTask(Stream receiveStream,int timeout, Action taskCompleteSuccessfulAction)
        {
            ThrowIfParamsIncorrect();
            if (TransferInProgress)
                throw new InvalidOperationException("Cannot run second transfer task until previos is not completed");

            transferTask = Task.Factory.StartNew(() => 
            { 
                TransferInProgress = false;
                {
                    int bufferSize = 64 * 64;
                    int readed = 0;
                    int readedAll = 0;
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

                if (!TransferDataTaskCancell.Token.IsCancellationRequested)
                    taskCompleteSuccessfulAction();
            });
        }

        public void RunSendTextDataTask(string text,TaskCompleteSuccesfull action)
        {
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
            RunSendStreamTask(stream, action);
        }
        
        public void RunSendStreamTask(Stream stream, TaskCompleteSuccesfull action)
        {
            ThrowIfParamsIncorrect();
            if (TransferInProgress)
                throw new InvalidOperationException("Cannot run second transfer task until previos is not completed");

            transferTask = Task.Factory.StartNew(() =>
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
                action?.Invoke(allReadedBytes);

            },this.TransferDataTaskCancell.Token);
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
