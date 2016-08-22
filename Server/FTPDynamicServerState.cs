using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharpFTP.Server
{
    public static class FTPDynamicServerState
    {
        public static IPAddress GetIp()
        {
            return address;
        }
        private static int port = 5000;
        private static IPAddress address = IPAddress.Loopback;

        public static int GetPasvPort()
        {
            return ++port;
        }

        public static void SetIp(IPAddress ipaddress)
        {
            address = ipaddress;
        }
    }
}
