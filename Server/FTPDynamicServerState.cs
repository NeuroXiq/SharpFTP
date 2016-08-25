using System.Net;
using System.Timers;
using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SharpFTP.Server
{
    public static class FTPDynamicServerState
    {
        public static IPAddress GetIp()
        {
            return address;
        }
        private static IPAddress address = IPAddress.Loopback;

        private static volatile object threadSafe = new object();
        private static Timer suspendTimer;
        private static Dictionary<int, bool> availablePorts;
        private static bool useCustomPorts;

        public static ushort MaximumConnections { get; private set; }
        public static ushort CurrentConnections { get; private set; }
        public static bool IsSuspended { get; private set; }
        /// <summary>
        /// Server suspend time in minutes
        /// </summary>
        public static int SuspendTime { get; private set; }


        static FTPDynamicServerState()
        {
            MaximumConnections = 10;
            IsSuspended = false;

            SuspendTime = 0;
            suspendTimer = new Timer();
            suspendTimer.Interval = 6000; // suspend timer should counting is minutes 
            suspendTimer.Elapsed += (sender,args) => UpdateSuspendStatus();

            useCustomPorts = false;
            availablePorts = new Dictionary<int, bool>();
        }

        private static void UpdateSuspendStatus()
        {
            if (SuspendTime > 0)
            {
                --SuspendTime;
            }
            else
            {
                ActivizeSuspendedServer();
            }
        }

        public static void SetMaximumConnections(ushort maxConnections)
        {
            MaximumConnections = maxConnections;
        }

        public static TcpListener GetPasvListener(out int port)
        {
            TcpListener listener;
            int freePort = -1;
            if (useCustomPorts)
            {
                lock (threadSafe)
                {
                    foreach (var p in availablePorts)
                    {
                        if (!p.Value)
                        {
                            freePort = p.Key;
                            availablePorts[p.Key] = true;
                            break;
                        }
                    }
                }

                listener = new TcpListener(IPAddress.Any, freePort);
                listener.Start(1);
                port = freePort;
            }
            else
            {
                listener = new TcpListener(IPAddress.Any, 0);
                listener.Start(1);
                port = (listener.LocalEndpoint as IPEndPoint).Port;
            }

            return listener;
        }

        public static void SetIp(IPAddress ipaddress)
        {
            address = ipaddress;
        }

        public static void SuspendServer(int time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException("Suspend time cannot be less than zero");
            if (time > 0)
            {
                if (IsSuspended)
                {
                    suspendTimer.Stop();
                }
                IsSuspended = true;
                suspendTimer.Start();
            }
        }

        public static void ActivizeSuspendedServer()
        {
            suspendTimer.Stop();
            IsSuspended = false;
            SuspendTime = 0;
        }

        public static void AddConnection()
        {
            lock (threadSafe)
            {
                ++CurrentConnections;
            }
        }

        public static void RemoveConnection()
        {
            lock (threadSafe)
            {
                if (CurrentConnections <= 0)
                    throw new Exception("Cannot decrement connection status when no connection exist");
                --CurrentConnections;  
            }
        }

        public static void RelasePort(int port)
        {
            if (useCustomPorts)
            {
                lock (threadSafe)
                {
                    availablePorts[port] = false;
                }
            }
        }

        public static void AddPasvPorts(int[] enablePortsRange)
        {
            if (MaximumConnections > enablePortsRange.Length)
            {
                throw new ArgumentOutOfRangeException("Maximum connections value cannot be greater than enable ports");
            }
            if (availablePorts.Count > 0)
            {
                throw new InvalidOperationException("Ports cannot be setted multiple times");
            }
            lock (threadSafe)
            {
                foreach (int port in enablePortsRange)
                {
                    availablePorts.Add(port, false);
                }
                useCustomPorts = true;
            }
        }
    }
}
