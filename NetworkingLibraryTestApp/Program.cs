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

        NetBase.DebugInfoReceived += NetBase_DebugInfoReceived;

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

        server.Send(2, "Example string!", 10);

        Thread.Sleep(-1);
    }

    private static void NetBase_DebugInfoReceived(string obj)
    {
        Console.WriteLine(obj);
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

public struct ExampleStruct
{
    public int IntValue;
    public string StringValue;
    public ExampleEnum CustomEnumValue;

    public ExampleStruct(int i, string s, ExampleEnum e)
    {
        IntValue = i;
        StringValue = s;
        CustomEnumValue = e;
    }
}

public enum ExampleEnum : ulong
{
    EnumValue1,
    EnumValue2,
    EnumValue3,
}