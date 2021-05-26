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
    public class UdpClient : NetBase
    {
#if DEBUG
        Random rand = new Random();
        public double DropChance = 0.0;
#endif

        private int bufferSize;
        private byte[] dataBuffer;
        internal DateTime lastMessageReceived = DateTime.UtcNow;

        public float TimeoutDelay = 20.0f;

        public event Action<UdpClient> ClientDisconnected;


        public UdpClient(uint secret = 0, int bufferSize = 1024) : base(SocketConfiguration.UdpConfiguration, secret, bufferSize)
        {
            this.bufferSize = bufferSize;
            systemDataEvents.Add(0, MethodInfoHelper.GetMethodInfo<UdpClient>(x => x.PingEventHandler(null)));
            systemDataEvents.Add(1, MethodInfoHelper.GetMethodInfo<UdpClient>(x => x.DisconnectEventHandler(null, false)));
            systemDataEvents.Add(2, MethodInfoHelper.GetMethodInfo<UdpClient>(x => x.ReliableDataResponseReceived(null, 0L)));
        }

        internal UdpClient(Socket serverSocket, EndPoint clientEp) : base(SocketConfiguration.UdpConfiguration, 0)
        {
            socket = serverSocket;
            endPoint = clientEp;
        }

        public bool VerifyAndListen(int port) => VerifyAndListen(IPAddress.Loopback, port);

        public bool VerifyAndListen(IPAddress remoteIp, int port)
        {
            try
            {
                endPoint = new IPEndPoint(remoteIp, port);
                byte[] verification = BitConverter.GetBytes(Secret);
                socket.SendTo(verification, 0, verification.Length, SocketFlags.None, endPoint);

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

                    if (BitConverter.ToUInt32(usableData.Skip(12).ToArray(), 0) == Secret)
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

        public override void Close()
        {
            try
            {
                SendF(1, PacketFlags.SystemMessage, true);
            } 
            catch { }

            //if (socket != null)
                DisconnectEventHandler(this);
        }

        private void DataReceivedEvent(IAsyncResult ar)
        {
            try
            {
                int i = socket.EndReceive(ar);

#if DEBUG
                // Simulated packet loss
                if (DropChance < rand.NextDouble())
                {
                    _ = ProcessData(dataBuffer.Take(i).ToArray());
                }
                else
                {
                    Console.WriteLine("Packet Dropped...");
                }
#else
                _ = ProcessData(dataBuffer.Take(i).ToArray());
#endif
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
                Close();
            }
        }

        private async Task ProcessData(byte[] data)
        {
            try 
            {
                NetBase.WriteDebug($"Client received data: {PacketToStringRep(data)}");

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
                    SendF(2, PacketFlags.SystemMessage, packetId);

                if (eventsRef.ContainsKey(eventId) && !receivedReliablePacketInfo.Contains(packetId))
                {
                    if (packetFlags.HasFlag(PacketFlags.Reliable))
                        if (!receivedReliablePacketInfo.Contains(packetId))
                        {
                            receivedReliableDataMutex.WaitOne();
                            receivedReliablePacketInfo.Add(packetId);
                            receivedReliableDataMutex.ReleaseMutex();
                        }

                    lastMessageReceived = DateTime.UtcNow;

                    MethodInfo netEventMethod = eventsRef[eventId];
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
                    Close();
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(TimeoutDelay / 4.0f));
            }
        }

        public override void SendRaw(byte eventId, PacketFlags flags, byte[] rawData, long? presetPacketId = null)
        {
            byte[] buffer = new byte[12 + rawData.Length];
            long packetId = presetPacketId.HasValue ? presetPacketId.Value : DateTime.UtcNow.Ticks;

            // PACKET HEADER CONSTRUCTION: 12 BYTES
            /* 0x00 1           EVENT_ID    */ buffer[0] = eventId;
            /* 0x01 1           FLAGS       */ buffer[1] = (byte)flags;
            /* 0x02 8           PACKET_ID   */ BitConverter.GetBytes(packetId).CopyTo(buffer, 2);
            /* 0x0A 2           DATA_LENGTH */ BitConverter.GetBytes((ushort)rawData.Length).CopyTo(buffer, 10);

            /* 0x0C DATA_LENGTH DATA        */ rawData.CopyTo(buffer, 12);

            try
            {
                socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, endPoint, new AsyncCallback(SendToEvent), null);
                NetBase.WriteDebug($"Send data to {IPEndPoint}: {PacketToStringRep(buffer)}");

                if (flags.HasFlag(PacketFlags.Reliable) && !flags.HasFlag(PacketFlags.ReservedA))
                {
                    sentReliableDataMutex.WaitOne();
                    sentReliablePacketInfo.Add(packetId);
                    sentReliableDataMutex.ReleaseMutex();
                    new Task(() => ResendReliable(new ReliablePacketInfo(eventId, flags, rawData, packetId))).Start();
                }
            }
            catch (Exception ex)
            {
                NetBase.WriteDebug(ex.ToString());
            }
        }

        internal async void ResendReliable(ReliablePacketInfo pi)
        {
            int resendAttempts = 0;
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(ReliableResendDelay));
                if (!sentReliablePacketInfo.Contains(pi.PacketID))
                    return;

                if (resendAttempts >= MaxResendAttempts)
                {
                    if (DisconnectOnFailedResponse)
                        Close();
                    return;
                }

                //PacketFlags nFlags = pi.Flags & ~PacketFlags.Reliable;
                SendRaw(pi.EventID, pi.Flags | PacketFlags.ReservedA, pi.PacketData, pi.PacketID);
                resendAttempts++;
            }
        }


        protected void PingEventHandler(UdpClient client) => SendF(0, PacketFlags.SystemMessage);

        protected void DisconnectEventHandler(UdpClient client, bool remoteTrigger = false)
        {
            if (ClientDisconnected != null)
                Array.ForEach(ClientDisconnected.GetInvocationList(), d => d.DynamicInvoke(this));

            sentReliableDataMutex.WaitOne();
            sentReliablePacketInfo.Clear();
            sentReliableDataMutex.ReleaseMutex();

            socket.Close();
        }

        protected void ReliableDataResponseReceived(UdpClient client, long packetID)
        {
            sentReliableDataMutex.WaitOne();
            if (sentReliablePacketInfo.Contains(packetID))
                sentReliablePacketInfo.Remove(packetID);
            sentReliableDataMutex.ReleaseMutex();
        }
    }
}
