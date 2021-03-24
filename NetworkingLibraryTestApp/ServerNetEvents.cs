using NetworkingLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkingLibraryTestApp
{
    public static class ServerNetEvents
    {
        [NetDataEvent(0, 0)]
        public static void BasicResponseEvent(NetBase n)
        {
            Console.WriteLine($"Received a packet from {n.IPEndPoint}!");
        }

        [NetDataEvent(1, 0)]
        public static void BasicSingleParamResponseEvent(NetBase n, byte[] ba)
        {
            Console.Write($"Received a packet from {n.IPEndPoint}:");
            for (int i = 0; i < ba.Length; ++i)
            {
                Console.Write($" {ba[i]}");
            }
            Console.WriteLine();
        }

        [NetDataEvent(2, 0)]
        public static void BasicMultiParamResponseEvent(NetBase n, byte[] ba, string s)
        {
            Console.Write($"Received a packet from {n.IPEndPoint}:");
            for (int i = 0; i < ba.Length; ++i)
            {
                Console.Write($" {ba[i]}");
            }
            Console.WriteLine();
            Console.WriteLine(s);
        }
    }
}
