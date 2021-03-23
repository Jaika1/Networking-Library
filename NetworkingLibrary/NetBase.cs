using NetworkingLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;

namespace NetworkingLibrary
{
    public abstract class NetBase
    {
        private readonly SocketConfiguration socketConfiguration;
        private readonly uint secret;
        protected Socket socket;
        protected Dictionary<byte, MethodInfo> netDataEvents;


        public uint Secret => secret;


        public NetBase(SocketConfiguration socketConfig, uint secret = 0)
        {
            this.secret = secret;
            socketConfiguration = socketConfig;
            socket = new Socket(socketConfig.AddressFamily, socketConfig.SocketType, socketConfig.ProtocolType);
        }


        public void Reset()
        {
            if (socket.Connected || socket.IsBound)
            {
                try
                {
                    socket.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            socket = new Socket(socketConfiguration.AddressFamily, socketConfiguration.SocketType, socketConfiguration.ProtocolType);
        }


        public virtual void Send(byte packetId, byte[] rawData)
            => throw new NotImplementedException("The inheriting class did not override this method! This is most certainly an oversight by the developer who created the inheriting class.");
    }
}
