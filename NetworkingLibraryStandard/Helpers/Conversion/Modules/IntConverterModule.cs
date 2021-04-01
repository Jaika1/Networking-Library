using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class IntConverterModule : IByteConverterModule
    {
        public Type T { get; } = typeof(int);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((int)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToInt32(data, 0);
    }
}
