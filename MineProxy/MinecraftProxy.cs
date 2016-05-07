using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MineProxy
{
    public class MinecraftProxy
    {
        private TcpListener _server;
        public MinecraftProxy(string serverIP, int port = 25565)
        {
            _server = new TcpListener(IPAddress.Any, 25565);
            _server.Start();
            while (true)
            {
                var clientHandle = new MinecraftClient(serverIP, port, _server.AcceptTcpClient());
                Thread.Sleep(500);
            }
        }
    }
}
