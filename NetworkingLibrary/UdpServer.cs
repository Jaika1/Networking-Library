using NetworkingLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NetworkingLibrary
{
    public class UdpServer : NetBase
    {
        private List<UdpClientReference> clientEndPoints = new List<UdpClientReference>();
        private EndPoint listenerEndPoint;

        public UdpServer(uint secret = 0) : base(SocketConfiguration.UdpConfiguration, secret) { }


        public void StartServer(int port)
        {
            listenerEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(listenerEndPoint);
            socket.BeginReceiveFrom();
        }
    }

    internal class UdpClientReference
    {
        internal EndPoint EndPoint;
        internal IPEndPoint IPEndPoint => (IPEndPoint)EndPoint;
        internal bool PingResponded = true;
    }
}
