using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetworkingLibrary.Extensions
{
    public struct SocketConfiguration
    {
        public static readonly SocketConfiguration TcpConfiguration = new SocketConfiguration(SocketType.Stream, ProtocolType.Tcp);
        public static readonly SocketConfiguration UdpConfiguration = new SocketConfiguration(SocketType.Dgram, ProtocolType.Udp);


        private AddressFamily addressFamily;
        private SocketType socketType;
        private ProtocolType protocolType;

        
        public AddressFamily AddressFamily => addressFamily;
        public SocketType SocketType => socketType;
        public ProtocolType ProtocolType => protocolType;


        public SocketConfiguration(SocketType socketType, ProtocolType protocolType) : this(AddressFamily.InterNetwork, socketType, protocolType) { }

        public SocketConfiguration(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
        }
    }
}
