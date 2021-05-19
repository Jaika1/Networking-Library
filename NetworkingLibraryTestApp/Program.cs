using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Jaika1.Networking;

class Program
{
    private static void Main()
    {
        //NetBase.DebugInfoReceived += (i) => Console.WriteLine(i);

        UdpServer sv = new UdpServer();
        sv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly());

        if (!sv.StartServer(7235))
            throw new Exception("Server failed to start!");

        for (ushort i = 0; i < 1; ++i)
        {
            UdpClient cl = new UdpClient();
#if DEBUG
            cl.DropChance = 0.01;
#endif

            cl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly());
            cl.ClientDisconnected += (c) => Console.WriteLine("Client Disconnected!");
            if (!cl.VerifyAndListen(7235))
                throw new Exception("Client failed to start!");

            cl.SendF(0, PacketFlags.Reliable, 0, i);
        }

        while(Console.ReadKey().Key != ConsoleKey.Escape) { }
        sv.Close();
    }

    [NetDataEvent(0)]
    static async void Increment(UdpClient client, int num, ushort offset)
    {
        Console.WriteLine(num++);
        client.SendF(0, PacketFlags.Reliable, num, offset);
    }
}