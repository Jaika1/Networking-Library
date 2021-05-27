using Jaika1.Networking.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Jaika1.Networking
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


        public UdpServer(uint secret = 0, int bufferSize = 1024) : base(SocketConfiguration.UdpConfiguration, secret, bufferSize)
        {
            this.bufferSize = bufferSize;
            systemDataEvents.Add(0, MethodInfoHelper.GetMethodInfo<UdpServer>(x => x.PingEventHandler(null)));
            systemDataEvents.Add(1, MethodInfoHelper.GetMethodInfo<UdpServer>(x => x.DisconnectEventHandler(null, false)));
            systemDataEvents.Add(2, MethodInfoHelper.GetMethodInfo<UdpServer>(x => x.ReliableDataResponseReceived(null, 0L)));
        }


        public bool StartServer(int bindPort) => StartServer(IPAddress.Any, bindPort);

        public bool StartServer(IPAddress bindIp, int bindPort)
        {
            try
            {
                endPoint = new IPEndPoint(bindIp, bindPort);
                socket.Bind(endPoint);
                dataBuffer = new byte[bufferSize];
                socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
                return false;
            }
            new Task(async () => await PingLoop(), cancellationToken.Token).Start();
            return true;
        }

        public override void Close()
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                DisconnectEventHandler(clientList[0]);
            }

            cancellationToken.Cancel();

            socket.Close();
        }


        private void DataReceivedEvent(IAsyncResult ar)
        {
            try
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int i = socket.EndReceiveFrom(ar, ref ep);
                new Task(async () => await ProcessData(dataBuffer.Take(i).ToArray(), ep)).Start();
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
            try
            {
                NetBase.WriteDebug($"Server received data: {PacketToStringRep(data)}");

                UdpClient clientRef = clientList.Find(c => c.EndPoint.Equals(clientEndPoint));
                if (clientRef == null)
                {
                    if (data.Length != 4 || BitConverter.ToUInt32(data, 0) != Secret)
                    {
                        NetBase.WriteDebug($"Client attempted to connect from {clientEndPoint} with a bad secret.");

                        return;
                    }
                    UdpClient rCl = new UdpClient(socket, clientEndPoint);

                    rCl.ReliableResendDelay = this.ReliableResendDelay;
                    rCl.MaxResendAttempts = this.MaxResendAttempts;
                    rCl.DisconnectOnFailedResponse = this.DisconnectOnFailedResponse;

                    rCl.ClientDisconnected += c => DisconnectEventHandler(c);
                    rCl.SendRaw(254, PacketFlags.None, BitConverter.GetBytes(Secret));
                    clientList.Add(rCl);

                    if (ClientConnected != null)
                        Array.ForEach(ClientConnected.GetInvocationList(), d => d.DynamicInvoke(rCl));
                }
                else
                {
                    if (data.Length < 12)
                        return;

                    byte eventId = data[0];
                    PacketFlags packetFlags = (PacketFlags)data[1];
                    long packetId = BitConverter.ToInt64(data, 2);
                    ushort dataLength = BitConverter.ToUInt16(data, 10);
                    byte[] netData = data.Skip(12).ToArray();

                    if (dataLength != netData.Length)
                        return;

                    Dictionary<byte, MethodInfo> eventsRef = packetFlags.HasFlag(PacketFlags.SystemMessage) ? systemDataEvents : netDataEvents;

                    if (packetFlags.HasFlag(PacketFlags.Reliable))
                        clientRef.SendF(2, PacketFlags.SystemMessage, packetId);


                    if (eventsRef.ContainsKey(eventId) && !clientRef.receivedReliablePacketInfo.Contains(packetId))
                    {
                        if (packetFlags.HasFlag(PacketFlags.Reliable))
                            if (!clientRef.receivedReliablePacketInfo.Contains(packetId))
                            {
                                lock (clientRef.receivedReliableDataLock)
                                {
                                    clientRef.receivedReliablePacketInfo.Add(packetId);
                                }
                            }

                        clientRef.lastMessageReceived = DateTime.UtcNow;

                        MethodInfo netEventMethod = eventsRef[eventId];
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
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
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

                SendF(0, PacketFlags.SystemMessage);

                await Task.Delay(TimeSpan.FromSeconds(PingFrequency));
            }
        }

        public override void SendRaw(byte packetId, PacketFlags flags, byte[] rawData, long? presetPacketId = null)
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                try
                {
                    clientList[i].SendRaw(packetId, flags, rawData, presetPacketId);
                }
                catch (Exception ex)
                {
                    NetBase.WriteDebug(ex.ToString());
                }
            }
        }


        protected void PingEventHandler(UdpClient client)
        {
            client.lastMessageReceived = DateTime.UtcNow;
        }

        protected void DisconnectEventHandler(UdpClient client, bool remoteTrigger = false)
        {
            if (!remoteTrigger)
                client.SendF(1, PacketFlags.SystemMessage);

            int cIndex = clientList.FindIndex(c => c == client);
            if (cIndex > -1)
                clientList.RemoveAt(cIndex);

            lock (client.sentReliableDataLock)
            {
                client.sentReliablePacketInfo.Clear();
            }

            if (ClientDisconnected != null)
                Array.ForEach(ClientDisconnected.GetInvocationList(), d => d.DynamicInvoke(client));
        }

        protected void ReliableDataResponseReceived(UdpClient client, long packetID)
        {
            lock (client.sentReliableDataLock)
            {
                if (client.sentReliablePacketInfo.Contains(packetID))
                    client.sentReliablePacketInfo.Remove(packetID);
            }
        }
    }
}
