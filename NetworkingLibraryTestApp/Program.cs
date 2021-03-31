//using NetworkingLibrary;
//using System;
//using System.Reflection;
//using System.Threading;

//namespace NetworkingLibraryTestApp
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            UdpServer udpSv = new UdpServer();
//            udpSv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
//            udpSv.ClientConnected += UdpSv_ClientConnected;
//            udpSv.ClientDisconnected += UdpSv_ClientDisconnected;
//            udpSv.StartServer(7235);

//            UdpClient udpCl = new UdpClient();
//            udpCl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
//            udpCl.ClientDisconnected += UdpCl_ClientDisconnected;
//            udpCl.VerifyAndListen(7235);

//            Thread.Sleep(4000);

//            udpSv.CloseServer();

//            Thread.Sleep(-1);
//        }
//        private static void UdpSv_ClientConnected(UdpClient obj)
//        {
//            Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
//        }

//        private static void UdpSv_ClientDisconnected(UdpClient obj)
//        {
//            Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
//        }

//        private static void UdpCl_ClientDisconnected(UdpClient obj)
//        {
//            Console.WriteLine($"Client disconnected from the server!");
//        }

//    }
//}

using System;
using System.Reflection;
using System.Threading;
using NetworkingLibrary;

class Program
{
    static void Main()
    {
        // Just a random port I've decided on, you can use whatever you want.
        int port = 7235;

        UdpServer server = new UdpServer();
        server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.StartServer(port);

        // Halt execution indefinitely so our application doesn't just immediately close.
        Thread.Sleep(-1);
    }

    static void Server_ClientConnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
    }

    static void Server_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
    }
}