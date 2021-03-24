using NetworkingLibrary;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UdpClient = NetworkingLibrary.UdpClient;

namespace NetworkingLibraryTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer udpSv = new UdpServer();
            udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
            udpSv.StartServer(7235);

            UdpClient udpCl = new UdpClient();
            udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
            udpCl.VerifyAndListen(new IPEndPoint(IPAddress.Loopback, 7235));
            DynamicPacket dp = new DynamicPacket();
            dp.AddData(0);
            udpCl.Send(0, dp);

            Thread.Sleep(-1);
        }
    }
}
