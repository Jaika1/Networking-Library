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
            Console.WriteLine("Received a packet!");
        }
    }
}
