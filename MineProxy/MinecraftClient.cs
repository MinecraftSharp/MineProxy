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
    public class MinecraftClient
    {
        private readonly TcpClient _targetServer;
        public MinecraftClient(string server, int ip, TcpClient clientServer)
        {
            _targetServer = new TcpClient(server, ip);
            new Task(() => {
                // Accept clients.
                while (true)
                {
                    try
                    {
                        new Task(async() =>
                        {
                            // Handle this client.
                            if (clientServer.Connected == false)
                            {
                                if (_targetServer.Connected)
                                    _targetServer.Close();
                                return;
                            }
                            var clientStream = clientServer.GetStream();
                            if (_targetServer.Connected == false)
                            {
                                try
                                {
                                    await _targetServer.ConnectAsync(Dns.GetHostAddresses(server)[0], ip);
                                }
                                catch
                                {
                                    await _targetServer.ConnectAsync(server, ip);
                                }
                            }
                            if (_targetServer.Connected == false)
                            {
                                if (clientServer.Connected)
                                    clientServer.Close();
                                return;
                            }
                            var serverStream = _targetServer.GetStream();
                            new Task(async() =>
                            {
                                var message = new byte[clientServer.ReceiveBufferSize];
                                //var message = new byte[46656];
                                while (true)
                                {
                                    int clientBytes;
                                    try
                                    {
                                        clientBytes = await clientStream.ReadAsync(message, 0, clientServer.ReceiveBufferSize);
                                    }
                                    catch
                                    {
                                        // Socket error - exit loop.  Client will have to reconnect.
                                        break;
                                    }
                                    if (clientBytes == 0)
                                    {
                                        // Client disconnected.
                                        break;
                                    }
                                    try
                                    {
                                        await serverStream.WriteAsync(Decode(message), 0, clientBytes);
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                }
                                //client.Close();
                            }).Start();
                            new Task(async() =>
                            {
                                var message = new byte[_targetServer.ReceiveBufferSize];
                                //var message = new byte[46656];
                                while (true)
                                {
                                    int serverBytes;
                                    try
                                    {
                                        serverBytes = await serverStream.ReadAsync(message, 0, _targetServer.ReceiveBufferSize);
                                        await clientStream.WriteAsync(Decode(message), 0, serverBytes);
                                    }
                                    catch
                                    {
                                        // Server socket error - exit loop.  Client will have to reconnect.
                                        break;
                                    }
                                    if (serverBytes == 0)
                                    {
                                        // server disconnected.
                                        break;
                                    }
                                }
                            }).Start();
                        }).Start();
                    }
                    catch
                    {
                        return;
                    }
                }
            }).Start();
        }

        public byte[] Decode(byte[] packet)
        {
            var i = packet.Length - 1;
            while (packet[i] == 0)
            {
                --i;
            }
            var temp = new byte[i + 1];
            Array.Copy(packet, temp, i + 1);
            return temp;
        }
    }
}
