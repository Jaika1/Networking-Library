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
        static void ClientArrayResponse(UdpClient client, string[] a)
        {
            Console.WriteLine("Data received from server:");
            Array.ForEach(a, i => Console.WriteLine(i));
            client.Send(3, (object)a);
        }
    }
}
