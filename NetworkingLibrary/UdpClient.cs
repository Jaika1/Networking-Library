using NetworkingLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace NetworkingLibrary
{
    public class UdpClient : NetBase
    {
        private int bufferSize;
        private byte[] dataBuffer;


        public UdpClient(int bufferSize = 1024, uint secret = 0) : base(SocketConfiguration.UdpConfiguration, secret)
        {
            this.bufferSize = bufferSize;
        }

        public UdpClient(Socket serverSocket, EndPoint clientEp) : base(SocketConfiguration.UdpConfiguration, 0)
        {
            socket = serverSocket;
            endPoint = clientEp;
        }


        public override void Send(byte packetId, byte[] rawData)
        {
            byte[] buffer = new byte[3 + rawData.Length];
            buffer[0] = packetId;
            BitConverter.GetBytes((ushort)rawData.Length).CopyTo(buffer, 1);
            rawData.CopyTo(buffer, 3);

            socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, endPoint, new AsyncCallback(SendToEvent), null);
        }

        private void SendToEvent(IAsyncResult ar)
        {
            try
            {
                socket.EndSendTo(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
