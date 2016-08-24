using SharpFTP.Server.FileSystem;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

namespace SharpFTP.Server
{
    public class FTPService
    {
        private TcpListener server;
        private ManualResetEvent wait;
        public int ConnectedClients { get; private set; }

        public FTPService(UserDataContext provider)
        {
            server = new TcpListener(IPAddress.Any,21);
            ServerDataContext.SetDataContext(provider);
            wait = new ManualResetEvent(false);
        }

        public void RunService()
        {            
            server.Start();
            while (true)
            {
                wait.Reset();
                
                server.BeginAcceptTcpClient(AcceptCallBack, server);

                wait.WaitOne();
            }
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            TcpListener acceptServer = (TcpListener)ar.AsyncState;
            TcpClient client = acceptServer.EndAcceptTcpClient(ar);
            wait.Set();
            HandleClient(client);
        }

        private void HandleClient(TcpClient client)
        {
            ++ConnectedClients;
            ClientSession session = new ClientSession(client);
            session.RunSession();
            --ConnectedClients;
        }
    }
}
