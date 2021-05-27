/*
PACKET LAYOUT:

HEADER: 12 BYTES

0x00 1           EVENT_ID
0x01 1           FLAGS
0x02 8           PACKET_ID
0x0A 2           DATA_LENGTH
0x0C DATA_LENGTH DATA

FLAGS:
0b_0000_0000 None
0b_0000_0001 Reliable
0b_0000_0010 SystemMessage
*/

using Jaika1.Networking.Helpers.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Jaika1.Networking
{
    public abstract class NetBase
    {
        private readonly SocketConfiguration socketConfiguration;
        private readonly uint secret;
        protected Socket socket;
        protected Dictionary<byte, MethodInfo> netDataEvents = new Dictionary<byte, MethodInfo>();
        protected Dictionary<byte, MethodInfo> systemDataEvents = new Dictionary<byte, MethodInfo>();
        protected EndPoint endPoint;
        protected ByteConverter converterInstance = new ByteConverter();
        protected CancellationTokenSource cancellationToken = new CancellationTokenSource();

        //internal Mutex sentReliableDataMutex = new Mutex();
        //internal Mutex receivedReliableDataMutex = new Mutex();
        internal object sentReliableDataLock = new object();
        internal object receivedReliableDataLock = new object();
        internal SortedSet<long> receivedReliablePacketInfo = new SortedSet<long>();
        internal SortedSet<long> sentReliablePacketInfo = new SortedSet<long>();
        public int MaxResendAttempts = 10;
        public bool DisconnectOnFailedResponse = true;
        public float ReliableResendDelay = 0.25f;

        public uint Secret => secret;
        public EndPoint EndPoint => endPoint;
        public IPEndPoint IPEndPoint => (IPEndPoint)endPoint;
        public ByteConverter Converter => converterInstance;

        public static event Action<string> DebugInfoReceived;

        public NetBase(SocketConfiguration socketConfig, uint secret = 0, int bufferSize = 1024)
        {
            this.secret = secret;
            socketConfiguration = socketConfig;
            socket = new Socket(socketConfig.AddressFamily, socketConfig.SocketType, socketConfig.ProtocolType);
            socket.ReceiveTimeout = 10000;
            socket.ReceiveBufferSize = bufferSize;
            socket.SendBufferSize = bufferSize;
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

        public static string PacketToStringRep(byte[] packet)
        {
            if (packet.Length < 12)
                return "INVALID PACKET SIZE";

            string retStr = "";
            /* 0x00 1           EVENT_ID       */ retStr += $"{packet[0].ToString("X2")} ";
            /* 0x01 1           FLAGS          */ retStr += $"{packet[1].ToString("X2")} ";
            /* 0x02 8           PACKET_ID      */ retStr += $"{string.Join("", packet.Skip(2).Take(8).Select(x => x.ToString("X2")))} ";
            /* 0x0A 2           DATA_LENGTH    */ retStr += $"{string.Join("", packet.Skip(10).Take(2).Select(x => x.ToString("X2")))} ";
            /* 0x0C DATA_LENGTH DATA           */ retStr += string.Join("", packet.Skip(12).Select(x => x.ToString("X2")));

            return retStr;
        }


        public void Send(byte packetId) => SendRaw(packetId, PacketFlags.None, new byte[0]);

        public void Send(byte packetId, params object[] data) => Send(packetId, new DynamicPacket(data));

        public void Send(byte packetId, DynamicPacket packet) => SendRaw(packetId, PacketFlags.None, packet.GetRawData(converterInstance));

        public void SendF(byte packetId, PacketFlags flags) => SendRaw(packetId, flags, new byte[0]);

        public void SendF(byte packetId, PacketFlags flags, params object[] data) => SendF(packetId, flags, new DynamicPacket(data));

        public void SendF(byte packetId, PacketFlags flags, DynamicPacket packet) => SendRaw(packetId, flags, packet.GetRawData(converterInstance));

        public abstract void SendRaw(byte packetId, PacketFlags flags, byte[] rawData, long? presetPacketId = null);

        public abstract void Close();

        //protected abstract void ResendReliable(ReliablePacketInfo packetInfo);
    }

    [Flags]
    public enum PacketFlags : byte
    {
        None          = 0b_0000_0000,
        Reliable      = 0b_0000_0001,
        SystemMessage = 0b_0000_0010,
        ReservedA     = 0b_1000_0000
    }

    public struct ReliablePacketInfo
    {
        public byte EventID;
        public PacketFlags Flags;
        public byte[] PacketData;
        public long PacketID;

        public ReliablePacketInfo(long packetId)
        {
            EventID = 0;
            Flags = PacketFlags.None;
            PacketID = packetId;
            PacketData = new byte[0];
        }

        public ReliablePacketInfo(byte eventId, PacketFlags flags, byte[] packetData, long packetId)
        {
            EventID = eventId;
            Flags = flags;
            PacketID = packetId;
            PacketData = packetData;
        }
    }
}
