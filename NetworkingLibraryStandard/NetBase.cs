﻿using NetworkingLibrary.Helpers.Conversion;
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
                    throw new Exception($"Attempted to add a new net event to this object with event id {attrib.EventId}, but there is already an event with this ID!");
                }
            }
        }

        public static void WriteDebug(string msg)
        {
            Debug.WriteLine(msg);
            Array.ForEach(DebugInfoReceived.GetInvocationList(), i => i.DynamicInvoke(msg));
        }


        public void Send(byte packetId) => SendRaw(packetId, new byte[0]);

        public void Send(byte packetId, params object[] data) => Send(packetId, new DynamicPacket(data));

        public void Send(byte packetId, DynamicPacket packet) => SendRaw(packetId, packet.GetRawData(converterInstance));

        internal virtual void SendRaw(byte packetId, byte[] rawData)
            => throw new NotImplementedException("The inheriting class did not override this method! This is most certainly an oversight by the developer who created the inheriting class.");
    }
}
