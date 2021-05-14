using NetworkingLibrary.Helpers.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace NetworkingLibrary
{
    public abstract class NetBase
    {
        private readonly SocketConfiguration socketConfiguration;
        private readonly uint secret;
        protected Socket socket;
        protected Dictionary<byte, MethodInfo> netDataEvents = new Dictionary<byte, MethodInfo>();
        protected EndPoint endPoint;
        protected ByteConverter converterInstance = new ByteConverter();

        protected sbyte redundantId = 0;
        protected List<sbyte> redundantIds = new List<sbyte>();

        public uint Secret => secret;
        public EndPoint EndPoint => endPoint;
        public IPEndPoint IPEndPoint => (IPEndPoint)endPoint;
        public ByteConverter Converter => converterInstance;

        public static event Action<string> DebugInfoReceived;

        public NetBase(SocketConfiguration socketConfig, uint secret = 0)
        {
            this.secret = secret;
            socketConfiguration = socketConfig;
            socket = new Socket(socketConfig.AddressFamily, socketConfig.SocketType, socketConfig.ProtocolType);
            socket.ReceiveTimeout = 10000;
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
                    WriteDebug(ex.ToString());
                }
            }
            socket = new Socket(socketConfiguration.AddressFamily, socketConfiguration.SocketType, socketConfiguration.ProtocolType);
        }

        public void AddNetEventsFromAssembly(Assembly asm, int eventGroupIdentifier = 0)
        {
            List<MethodInfo> netEventGroupMethods = (from t in asm.GetTypes()
                                                     from m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                     where m.GetCustomAttribute<NetDataEventAttribute>() != null
                                                     where m.GetCustomAttribute<NetDataEventAttribute>().EventGroupIdentifier == eventGroupIdentifier
                                                     where m.GetParameters().Length > 0
                                                     where m.GetParameters()[0].ParameterType == typeof(NetBase) || m.GetParameters()[0].ParameterType.BaseType == typeof(NetBase)
                                                     select m).ToList();

            for(int i = 0; i < netEventGroupMethods.Count; ++i)
            {
                MethodInfo nem = netEventGroupMethods[i];
                NetDataEventAttribute attrib = nem.GetCustomAttribute<NetDataEventAttribute>();
                if (!netDataEvents.ContainsKey(attrib.EventId))
                {
                    netDataEvents.Add(attrib.EventId, nem);
                }
                else
                {
                    NetBase.WriteDebug($"Attempted to add a new net event to this object with event id {attrib.EventId}, but there is already an event with this ID! Make sure you're not adding the same event twice within the same group ID, and that no events use id 254 or 255!", true);
                }
            }
        }

        public static void WriteDebug(string msg, bool throwException = false)
        {
            Debug.WriteLine(msg);

            if (DebugInfoReceived != null)
                Array.ForEach(DebugInfoReceived.GetInvocationList(), i => i.DynamicInvoke(msg));

            if (throwException)
                throw new Exception(msg);
        }


        public void Send(byte packetId) => SendRaw(packetId, false, new byte[0]);

        public void Send(byte packetId, params object[] data) => Send(packetId, new DynamicPacket(data));

        public void Send(byte packetId, DynamicPacket packet) => SendRaw(packetId, false, packet.GetRawData(converterInstance));

        public void SendRedundant(byte packetId) => SendRaw(packetId, true, new byte[0]);

        public void SendRedundant(byte packetId, params object[] data) => SendRedundant(packetId, new DynamicPacket(data));

        public void SendRedundant(byte packetId, DynamicPacket packet) => SendRaw(packetId, true, packet.GetRawData(converterInstance));

        internal virtual void SendRaw(byte packetId, bool redundant, byte[] rawData)
            => NetBase.WriteDebug("The inheriting class did not override this method! This is most certainly an oversight by the developer who created the inheriting class. (From NetBase)", true);
    }
}
