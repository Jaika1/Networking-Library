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
            udpSv.ClientConnected += UdpSv_ClientConnected;
            udpSv.ClientDisconnected += UdpSv_ClientDisconnected;
            udpSv.StartServer(7235);

            UdpClient udpCl = new UdpClient();
            udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
            udpCl.ClientDisconnected += UdpCl_ClientDisconnected;
            udpCl.VerifyAndListen(new IPEndPoint(IPAddress.Loopback, 7235));

            Thread.Sleep(4000);

            udpSv.CloseServer();

            Thread.Sleep(-1);
        }
        private static void UdpSv_ClientConnected(UdpClient obj)
        {
            Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
        }

        private static void UdpSv_ClientDisconnected(UdpClient obj)
        {
            Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
        }

        private static void UdpCl_ClientDisconnected(UdpClient obj)
        {
            Console.WriteLine($"Client disconnected from the server!");
        }

    }
}
