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
            udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
            udpSv.StartServer(7235);

            UdpClient udpCl = new UdpClient();
            udpCl.VerifyAndListen(new IPEndPoint(IPAddress.Loopback, 7235));
            udpCl.Send(0);
            udpCl.Send(1, new byte[] { 2, 4, 6, 8 });

            DynamicPacket dp = new DynamicPacket();
            dp.AddData(new byte[] { 4, 8, 16, 32 });
            dp.AddData("Nice");
            udpCl.Send(2, dp);

            Thread.Sleep(-1);
        }
    }
}
