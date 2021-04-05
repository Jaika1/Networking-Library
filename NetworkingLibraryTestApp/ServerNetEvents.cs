using NetworkingLibrary;
using System;

namespace NetworkingLibraryTestApp
{
    internal static class ServerNetEvents
    {
        [NetDataEvent(0)]
        static void ServerNoDataResponse(UdpClient sender)
        {
            Console.WriteLine($"{sender.IPEndPoint,20}] Dataless message received from client!");
        }

        [NetDataEvent(1)]
        static void ServerBooleanResponse(UdpClient sender, bool b)
        {
            Console.WriteLine($"{sender.IPEndPoint,20}] Boolean with value \"{b}\" received from the client!");
        }

        [NetDataEvent(2)]
        static void ServerMultiTypeResponse(UdpClient sender, string s, int i)
        {
            Console.WriteLine($"{sender.IPEndPoint,20}] String with value \"{s}\" and int with value \"{i}\" received from the client!");
        }

        [NetDataEvent(3)]
        static void ServerArrayResponse(UdpClient sender, string[] sa, int[] ia)
        {
            Console.WriteLine("Data received from client (string):");
            Array.ForEach(sa, i => Console.WriteLine(i));
            Console.WriteLine("Data received from client (int):");
            Array.ForEach(ia, i => Console.WriteLine(i));
            Console.WriteLine();
        }

        [NetDataEvent(4)]
        static void ServerEnumResponse(UdpClient sender, ExampleEnum e)
        {
            Console.WriteLine($"{sender.IPEndPoint,20}] Enum with value \"{e}\" received from the client!");
        }
    }
}
