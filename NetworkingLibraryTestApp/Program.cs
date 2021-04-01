using System;
using System.Reflection;
using System.Threading;
using NetworkingLibrary;
using NetworkingLibrary.Helpers.Conversion;

class Program
{
    //static void Main()
    //{
    //    // Just a random port I've decided on, you can use whatever you want.
    //    int port = 7235;

    //    UdpServer server = new UdpServer();
    //    server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
    //    server.ClientConnected += Server_ClientConnected;
    //    server.ClientDisconnected += Server_ClientDisconnected;
    //    server.StartServer(port);

    //    UdpClient client = new UdpClient();
    //    client.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1); // Take note that we've updated the group ID here to 1!
    //    client.ClientDisconnected += Client_ClientDisconnected;
    //    client.VerifyAndListen(port);

    //    // Send a "dummy" message to all connected clients
    //    server.Send(0);

    //    // Halt execution indefinitely so our application doesn't just immediately close.
    //    Thread.Sleep(-1);
    //}

    static void Main()
    {
        ByteConverter converter = new ByteConverter();

        string b1 = "yippe yippe";

        byte[] d = converter.ConvertToBytes(b1, false);

        string b2 = (string)converter.ObjectFromBytes(b1.GetType(), d);
        Console.WriteLine($"{b1,-24}|{b2, 24}");
    }

    // Event responding methods from before
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

    // Net events for our server and client
    [NetDataEvent(0, 0)]
    static void PrintPong(UdpClient client)
    {
        Console.WriteLine("Pong!");
    }

    [NetDataEvent(0, 1)]
    static void PrintPingAndRespond(UdpClient client)
    {
        Console.WriteLine("Ping!");
        client.Send(0);
    }
}