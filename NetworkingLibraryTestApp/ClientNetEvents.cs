using NetworkingLibrary;
using System;

namespace NetworkingLibraryTestApp
{
    internal static class ClientNetEvents
    {
        [NetDataEvent(0, 1)]
        static void ClientNoDataResponse(UdpClient client)
        {
            Console.WriteLine($"{client.IPEndPoint,20}] Dataless message received from the server! Will respond with the same data!");
            client.Send(0);
        }

        [NetDataEvent(1, 1)]
        static void ClientBooleanResponse(UdpClient client, bool b)
        {
            Console.WriteLine($"{client.IPEndPoint,20}] Boolean with value \"{b}\" received from the server! Will respond with the same data!");
            client.Send(1, b);
        }

        [NetDataEvent(2, 1)]
        static void ClientMultiTypeResponse(UdpClient client, string s, int i)
        {
            Console.WriteLine($"{client.IPEndPoint,20}] String with value \"{s}\" and int with value \"{i}\" received from the server! Will respond with the same data!");
            client.Send(2, s, i);
        }

        [NetDataEvent(3, 1)]
        static void ClientArrayResponse(UdpClient client, string[] sa, int[] ia)
        {
            Console.WriteLine("Data received from server (string):");
            Array.ForEach(sa, i => Console.WriteLine(i));
            Console.WriteLine("Data received from server (int):");
            Array.ForEach(ia, i => Console.WriteLine(i));
            Console.WriteLine();
            client.Send(3, sa, ia);
        }

        [NetDataEvent(4, 1)]
        static void ClientEnumResponse(UdpClient client, ExampleEnum e)
        {
            Console.WriteLine($"{client.IPEndPoint,20}] Enum with value \"{e}\" received from the server! Will respond with the same data!");
            client.Send(4, e);
        }

        [NetDataEvent(5, 1)]
        static void ClientObjectResponse(UdpClient client, ExampleStruct s)
        {
            Console.WriteLine($"{client.IPEndPoint,20}] Enum with value \"{s}\" received from the server! Will respond with the same data!");
            client.Send(5, s);
        }
    }
}
