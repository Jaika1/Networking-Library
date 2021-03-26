using NetworkingLibrary.Extensions;
using System;
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

        public UdpClient(Socket serverSocket, EndPoint clientEp) : base(SocketConfiguration.UdpConfiguration, 0)
        {
            socket = serverSocket;
            endPoint = clientEp;
        }

        
        public bool VerifyAndListen(IPEndPoint serverEndPoint)
        {
            endPoint = serverEndPoint;
            byte[] verification = BitConverter.GetBytes(Secret);
            socket.SendTo(verification, 0, verification.Length, SocketFlags.None, endPoint);

            byte[] response = new byte[7];
            try
            {
                socket.ReceiveFrom(response, 0, response.Length, SocketFlags.None, ref endPoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }

            if (BitConverter.ToUInt32(response.Skip(3).ToArray(), 0) != Secret)
            {
                Debug.WriteLine("Verification response was incorrect!");
                return false;
            }

            dataBuffer = new byte[bufferSize];
            try
            {
                socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            _ = TimeoutLoop();

            return true;
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
                Debug.WriteLine(ex.ToString());
            }

            dataBuffer = new byte[bufferSize];
            try
            {
                socket.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref endPoint, new AsyncCallback(DataReceivedEvent), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task ProcessData(byte[] data)
        {
            if (data.Length < 3) return;

            byte eventId = data[0];
            ushort dataLength = BitConverter.ToUInt16(data, 1); // Used here for error checking.
            byte[] usefulData = data.Skip(3).ToArray();
            if (usefulData.Length != dataLength) return;

            if (netDataEvents.ContainsKey(eventId))
            {
                lastMessageReceived = DateTime.UtcNow;

                MethodInfo netEventMethod = netDataEvents[eventId];
                ParameterInfo[] parameters = netEventMethod.GetParameters().Skip(1).ToArray();
                if (parameters.Length == 0)
                {
                    netEventMethod.Invoke(netEventMethod.IsStatic ? null : this, new object[] { this });
                }
                else if (parameters.Length == 1)
                {
                    object o = DynamicPacket.ByteArrayToObject(usefulData);
                    netEventMethod.Invoke(netEventMethod.IsStatic ? null : this, new object[] { this, o });
                }
                else
                {
                    object[] objects = new object[1 + parameters.Length];
                    objects[0] = this;
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        ushort paramDataLength = BitConverter.ToUInt16(usefulData, 0);
                        byte[] paramData = usefulData.Skip(2).Take(paramDataLength).ToArray();
                        objects[1 + i] = DynamicPacket.ByteArrayToObject(paramData);
                        usefulData = usefulData.Skip(2 + paramDataLength).ToArray();
                    }
                    netEventMethod.Invoke(netEventMethod.IsStatic ? null : this, objects);
                }
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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
