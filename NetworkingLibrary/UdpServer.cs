using NetworkingLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace NetworkingLibrary
{
    public class UdpServer : NetBase
    {
        private List<UdpClient> clientList = new List<UdpClient>();
        private int bufferSize;
        private byte[] dataBuffer;

        public UdpServer(int bufferSize = 1024, uint secret = 0) : base(SocketConfiguration.UdpConfiguration, secret)
        {
            this.bufferSize = bufferSize;
        }


        public void StartServer(int port)
        {
            endPoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(endPoint);
            dataBuffer = new byte[bufferSize];
            socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
        }

        private void DataReceivedEvent(IAsyncResult ar)
        {
            try
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int i = socket.EndReceiveFrom(ar, ref ep);
                _ = ProcessData(dataBuffer.Take(i).ToArray(), ep);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            dataBuffer = new byte[bufferSize];
            socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
        }

        private async Task ProcessData(byte[] data, EndPoint clientEndPoint)
        {
            UdpClient clientRef = clientList.Find(c => c.EndPoint.Equals(clientEndPoint));
            if (clientRef == null)
            {
                // Proper verification using the secret later.
                UdpClient rCl = new UdpClient(socket, clientEndPoint);
                rCl.Send(0, new byte[] { 1 });
                clientList.Add(rCl);
            }
            else
            {
                if (data.Length < 3) return;

                byte eventId = data[0];
                ushort dataLength = BitConverter.ToUInt16(data, 1); // Used here for error checking.
                byte[] usefulData = data.Skip(3).ToArray();
                if (usefulData.Length != dataLength) return;

                if (netDataEvents.ContainsKey(eventId))
                {
                    MethodInfo netEventMethod = netDataEvents[eventId];
                    ParameterInfo[] parameters = netEventMethod.GetParameters().Skip(1).ToArray();
                    if (parameters.Length == 0)
                    {
                        netEventMethod.Invoke(null, new object[] { clientRef });
                    }
                    else if (parameters.Length == 1)
                    {
                        object o = DynamicPacket.ByteArrayToObject(usefulData);
                        netEventMethod.Invoke(null, new object[] { clientRef, o });
                    }
                    else
                    {
                        object[] objects = new object[1 + parameters.Length];
                        objects[0] = clientRef;
                        for (int i = 0; i < parameters.Length; ++i)
                        {
                            ushort paramDataLength = BitConverter.ToUInt16(usefulData, 0);
                            byte[] paramData = usefulData.Skip(2).Take(paramDataLength).ToArray();
                            objects[1 + i] = DynamicPacket.ByteArrayToObject(paramData);
                            usefulData = usefulData.Skip(2 + paramDataLength).ToArray();
                        }
                        netEventMethod.Invoke(null, objects);
                    }
                }
            }
        }

        internal override void SendRaw(byte packetId, byte[] rawData)
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                clientList[i].SendRaw(packetId, rawData);
            }
        }
    }
}
