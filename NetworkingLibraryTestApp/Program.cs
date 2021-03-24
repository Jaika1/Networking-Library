using NetworkingLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace NetworkingLibraryTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer udpSv = new UdpServer();
            udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
            udpSv.StartServer(7235);


            Thread.Sleep(-1);
        }
    }
}
