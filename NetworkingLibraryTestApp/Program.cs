using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkingLibrary;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);


    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutput([In] IntPtr cOut, [In] CHAR_INFO[] cInfo, [In] COORD bufferSize, [In] COORD writeCoord, [In, Out] ref SMALL_RECT rect);

    [DllImport("kernel32.dll")]
    static extern int GetLastError();

    static IntPtr outHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

    private static void Main()
    {
        NetBase.DebugInfoReceived += (i) => Console.WriteLine(i);

        UdpServer sv = new UdpServer();
        sv.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly());
        if (!sv.StartServer(7235))
            throw new Exception("Server failed to start!");

        for (ushort i = 0; i < 1; ++i)
        {
            UdpClient cl = new UdpClient();
#if DEBUG
            cl.DropChance = 0.1;
#endif
            cl.DisconnectOnFailedResponse = false;
            cl.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly());
            cl.ClientDisconnected += (c) => Console.WriteLine("Client Disconnected!");
            if (!cl.VerifyAndListen(7235))
                throw new Exception("Client failed to start!");

            cl.SendF(0, PacketFlags.Reliable, 0, i);
        }

        while(Console.ReadKey().Key != ConsoleKey.Escape) { }
        sv.Close();
    }



    //[NetDataEvent(0), STAThread]
    //static void Increment(UdpClient client, int num, ushort offset)
    //{
    //    num++;

    //    CHAR_INFO[] cArr = CHAR_INFO.StringToCharInfo($"{offset,4}: {num.ToString()}");
    //    COORD pos = new COORD() { X = 0, Y = 0 };
    //    COORD bSize = new COORD() { X = (ushort)cArr.Length, Y = 1 };
    //    SMALL_RECT rect = new SMALL_RECT() { Left = 1, Top = offset, Right = bSize.X, Bottom = (ushort)(offset + bSize.Y) };

    //    WriteConsoleOutput(outHandle, cArr, bSize, pos, ref rect);
    //    client.SendF(0, PacketFlags.Reliable, num, offset);
    //}

    [NetDataEvent(0)]
    static async void Increment(UdpClient client, int num, ushort offset)
    {
        Console.WriteLine(num++);
        await Task.Delay(100);
        client.SendF(0, PacketFlags.Reliable, num, offset);
    }

    [StructLayout(LayoutKind.Explicit)]
    struct CHAR_INFO
    {
        [FieldOffset(0)] public CHAR_UNION Char;
        [FieldOffset(2)] public ushort Attributes;

        public static CHAR_INFO[] StringToCharInfo(string s)
        {
            return (from c in s select new CHAR_INFO() { Char = c, Attributes = 0x0007 }).ToArray();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct CHAR_UNION
    {
        [FieldOffset(0)] public byte AnsiChar;
        [FieldOffset(0)] public char UnicodeChar;

        public static implicit operator CHAR_UNION(char c)
        {
            return new CHAR_UNION()
            {
                AnsiChar = BitConverter.GetBytes(c)[0],
                UnicodeChar = c
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct COORD
    {
        [MarshalAs(UnmanagedType.U2)] public ushort X;
        [MarshalAs(UnmanagedType.U2)] public ushort Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SMALL_RECT
    {
        [MarshalAs(UnmanagedType.U2)] public ushort Left;
        [MarshalAs(UnmanagedType.U2)] public ushort Top;
        [MarshalAs(UnmanagedType.U2)] public ushort Right;
        [MarshalAs(UnmanagedType.U2)] public ushort Bottom;
    }
}