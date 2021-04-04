using System;
using System.Reflection;
using System.Threading;
using NetworkingLibrary;

class Program
{
    //static void Main()
    //{
    //    ByteConverter converter = new ByteConverter();

    //    int[] b1 = new[] { 12, -4321, 123456 };

    //    byte[] d = converter.ConvertToBytes(b1, false);

    //    int[] b2 = ((object[])converter.ObjectFromBytes(b1.GetType(), d).Instance).Cast<int>().ToArray();
    //    Console.WriteLine($"{b1,-24}|{b2,24}");
    //}

    static void Main()
    {
        int port = 7235;

        UdpServer server = new UdpServer();
        server.TimeoutDelay = 10000.0f;
        server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.StartServer(port);

        UdpClient client = new UdpClient();
        client.TimeoutDelay = 10000.0f;
        client.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1);
        client.ClientDisconnected += Client_ClientDisconnected;
        client.VerifyAndListen(port);

        //// Dataless
        //server.Send(0);
        //Thread.Sleep(20);

        //// Boolean
        //server.Send(1, true);
        //Thread.Sleep(20);

        //// Multiple types
        //server.Send(2, "Hello World!", 37);
        //Thread.Sleep(20);

        server.Send(4, ByteEnum.Ass);

        Thread.Sleep(-1);
    }

    // Event responding methods
    static void Server_ClientConnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
    }

    static void Server_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
    }

    static void Client_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client instance has been disconnected from the server!");
    }
}

public enum ByteEnum : byte
{
    Piss,
    Ass
}