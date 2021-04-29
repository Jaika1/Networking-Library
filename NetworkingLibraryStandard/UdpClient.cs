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
    public class UdpClient : NetBase
    {
        private int bufferSize;
        private byte[] dataBuffer;
        internal DateTime lastMessageReceived = DateTime.UtcNow;

        public float TimeoutDelay = 20.0f;

        public event Action<UdpClient> ClientDisconnected;


        public UdpClient(uint secret = 0, int bufferSize = 1024) : base(SocketConfiguration.UdpConfiguration, secret)
        {
            this.bufferSize = bufferSize;
            netDataEvents.Add(254, MethodInfoHelper.GetMethodInfo<UdpClient>(x => x.PingEventHandler(null)));
            netDataEvents.Add(255, MethodInfoHelper.GetMethodInfo<UdpClient>(x => x.DisconnectEventHandler(null, false)));
        }

        internal UdpClient(Socket serverSocket, EndPoint clientEp) : base(SocketConfiguration.UdpConfiguration, 0)
        {
            socket = serverSocket;
            endPoint = clientEp;
        }

        public bool VerifyAndListen(int port) => VerifyAndListen(IPAddress.Loopback, port);

        public bool VerifyAndListen(IPAddress remoteIp, int port)
        {
            endPoint = new IPEndPoint(remoteIp, port);
            byte[] verification = BitConverter.GetBytes(Secret);
            socket.SendTo(verification, 0, verification.Length, SocketFlags.None, endPoint);

            try
            {
                int attempts = 0;
                List<byte[]> nonData = new List<byte[]>(); // Used to store non-verif data that may be received first
                while (attempts <= 5)
                {
                    attempts++;
                    byte[] response = new byte[bufferSize];
                    int bytesReceived = socket.ReceiveFrom(response, 0, response.Length, SocketFlags.None, ref endPoint);
                    byte[] usableData = response.Take(bytesReceived).ToArray();

                    if (usableData[0] != 254 && BitConverter.ToUInt16(usableData, 1) != 4)
                    {
                        NetBase.WriteDebug($"Non-verification data received, will store for future processing. ({attempts}/5 attempts)");
                        nonData.Add(usableData);
                        continue;
                    }

                    if (BitConverter.ToUInt32(usableData.Skip(3).ToArray(), 0) == Secret)
                    {
                        NetBase.WriteDebug($"Verification successful, will now listen for other data...");

                        dataBuffer = new byte[bufferSize];
                        try
                        {
                            socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
                        }
                        catch (Exception ex)
                        {
                            NetBase.WriteDebug(ex.ToString());
                            return false;
                        }
                        _ = TimeoutLoop();

                        // Process the non-verif data received beforehand
                        nonData.ForEach(d =>
                        {
                            _ = ProcessData(d);
                        });

                        return true;
                    }
                    else
                    {
                        NetBase.WriteDebug($"Verification response was incorrect! Received: {string.Join(" ", response)}");
                        return false;
                    }
                }
                NetBase.WriteDebug($"Verification attempt limit reached!");
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }

            return false;
        }

        public void Disconnect()
        {
            Send(255, new DynamicPacket(true));
            DisconnectEventHandler(this);
        }

        private void DataReceivedEvent(IAsyncResult ar)
        {
            try
            {
                int i = socket.EndReceive(ar);
                _ = ProcessData(dataBuffer.Take(i).ToArray());
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

        private async Task ProcessData(byte[] data)
        {
            try 
            {
                NetBase.WriteDebug($"Client received data: {string.Join(" ", data)}");

                if (data.Length < 3)
                    return;

                byte eventId = data[0];
                ushort dataLength = BitConverter.ToUInt16(data, 1);
                byte[] netData = data.Skip(3).ToArray();
                if (dataLength != netData.Length)
                    return;

                if (netDataEvents.ContainsKey(eventId))
                {
                    lastMessageReceived = DateTime.UtcNow;

                    MethodInfo netEventMethod = netDataEvents[eventId];
                    ParameterInfo[] parameters = netEventMethod.GetParameters().Skip(1).ToArray();
                    Type[] parameterTypes = (from p in parameters
                                             select p.ParameterType).ToArray();

                    object[] instances = DynamicPacket.GetInstancesFromData(netData, converterInstance, parameterTypes);

                    object[] instancesWithNetBase = new object[1 + instances.Length];
                    instancesWithNetBase[0] = this;
                    instances.CopyTo(instancesWithNetBase, 1);
                    netEventMethod.Invoke(netEventMethod.IsStatic ? null : this, instancesWithNetBase);
                }
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }
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

        private async Task TimeoutLoop()
        {
            while (true)
            {
                if ((DateTime.UtcNow - lastMessageReceived).TotalSeconds >= TimeoutDelay)
                {
                    Disconnect();
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(TimeoutDelay / 4.0f));
            }
        }

        internal override void SendRaw(byte packetId, byte[] rawData)
        {
            byte[] buffer = new byte[3 + rawData.Length];
            buffer[0] = packetId;
            BitConverter.GetBytes((ushort)rawData.Length).CopyTo(buffer, 1);
            rawData.CopyTo(buffer, 3);

            try
            {
                socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, endPoint, new AsyncCallback(SendToEvent), null);
                NetBase.WriteDebug($"Send data to {IPEndPoint}: {string.Join(" ", buffer)}");
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }
        }

        protected virtual void PingEventHandler(UdpClient client) => Send(254);

        protected virtual void DisconnectEventHandler(UdpClient client, bool remoteTrigger = false)
        {
            if (ClientDisconnected != null)
                Array.ForEach(ClientDisconnected.GetInvocationList(), d => d.DynamicInvoke(this));

            socket.Close();
        }
    }
}
