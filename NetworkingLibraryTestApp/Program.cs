using System;
using System.Reflection;
using System.Threading;
using NetworkingLibrary;

class Program
{
    private static void Main()
    {
        sbyte num = 0;
        while (true)
        {
            Console.WriteLine(num++);
            if (num + 1 > sbyte.MaxValue)
                num = 0;
        }
    }
}