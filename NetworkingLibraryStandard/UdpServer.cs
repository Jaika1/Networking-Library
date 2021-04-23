using NetworkingLibrary.Helpers;
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

        public float PingFrequency = 8.0f;
        public float TimeoutDelay = 20.0f;

        public event Action<UdpClient> ClientConnected;
        public event Action<UdpClient> ClientDisconnected;


        public IReadOnlyList<UdpClient> Clients => clientList;


        public UdpServer(uint secret = 0, int bufferSize = 1024) : base(SocketConfiguration.UdpConfiguration, secret)
        {
            this.bufferSize = bufferSize;
            netDataEvents.Add(254, MethodInfoHelper.GetMethodInfo<UdpServer>(x => x.PingEventHandler(null)));
            netDataEvents.Add(255, MethodInfoHelper.GetMethodInfo<UdpServer>(x => x.DisconnectEventHandler(null, false)));
        }


        public void StartServer(int bindPort) => StartServer(IPAddress.Any, bindPort);

        public void StartServer(IPAddress bindIp, int bindPort)
        {
            endPoint = new IPEndPoint(bindIp, bindPort);
            socket.Bind(endPoint);
            dataBuffer = new byte[bufferSize];
            try
            {
                socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }
            _ = PingLoop();
        }

        public void CloseServer()
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                DisconnectEventHandler(clientList[0], true);
            }

            socket.Close();
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
                NetBase.WriteDebug(ex.ToString());
            }

            dataBuffer = new byte[bufferSize];
            try
            {
                socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }
        }

        private async Task ProcessData(byte[] data, EndPoint clientEndPoint)
        {
            UdpClient clientRef = clientList.Find(c => c.EndPoint.Equals(clientEndPoint));
            if (clientRef == null)
            {
                if (data.Length != 4 || BitConverter.ToUInt32(data, 0) != Secret) 
                {
                    NetBase.WriteDebug($"Client attempted to connect from {clientEndPoint} with a bad secret.");
                    return;
                }
                UdpClient rCl = new UdpClient(socket, clientEndPoint);
                rCl.SendRaw(254, BitConverter.GetBytes(Secret));
                clientList.Add(rCl);

                if (ClientConnected != null)
                    Array.ForEach(ClientConnected.GetInvocationList(), d => d.DynamicInvoke(rCl));
            }
            else
            {
                if (data.Length < 3)
                    return;

                byte eventId = data[0];
                ushort dataLength = BitConverter.ToUInt16(data, 1);
                byte[] netData = data.Skip(3).ToArray();
                if (dataLength != netData.Length)
                    return;

                if (netDataEvents.ContainsKey(eventId))
                {
                    clientRef.lastMessageReceived = DateTime.UtcNow;

                    MethodInfo netEventMethod = netDataEvents[eventId];
                    ParameterInfo[] parameters = netEventMethod.GetParameters().Skip(1).ToArray();
                    Type[] parameterTypes = (from p in parameters
                                             select p.ParameterType).ToArray();

                    object[] instances = DynamicPacket.GetInstancesFromData(netData, converterInstance, parameterTypes);
                    object[] instancesWithNetBase = new object[1 + instances.Length];
                    instancesWithNetBase[0] = clientRef;
                    instances.CopyTo(instancesWithNetBase, 1);
                    netEventMethod.Invoke(netEventMethod.IsStatic ? null : this, instancesWithNetBase);
                }
            }
        }

        private async Task PingLoop()
        {
            while (true)
            {
                for  (int i = 0; i < clientList.Count; ++i)
                {
                    UdpClient client = clientList[i];
                    if ((DateTime.UtcNow - client.lastMessageReceived).TotalSeconds >= TimeoutDelay)
                    {
                        DisconnectEventHandler(client);
                        --i;
                    }
                }

                Send(254);

                await Task.Delay(TimeSpan.FromSeconds(PingFrequency));
            }
        }

        internal override void SendRaw(byte packetId, byte[] rawData)
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                try
                {
                    clientList[i].SendRaw(packetId, rawData);
                }
                catch (Exception ex)
                {
                    NetBase.WriteDebug(ex.ToString());
                }
            }
        }


        protected virtual void PingEventHandler(UdpClient client)
        {
            client.lastMessageReceived = DateTime.UtcNow;
        }

        protected virtual void DisconnectEventHandler(UdpClient client, bool remoteTrigger = false)
        {
            if (!remoteTrigger)
                client.Send(255);

            int cIndex = clientList.FindIndex(c => c == client);
            if (cIndex > -1)
                clientList.RemoveAt(cIndex);

            if (ClientDisconnected != null)
                Array.ForEach(ClientDisconnected.GetInvocationList(), d => d.DynamicInvoke(client));
        }
    }
}
