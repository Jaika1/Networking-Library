using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class UShortConverterModule : IByteConverterModule
    {
        public Type T { get; } = typeof(ushort);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((ushort)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToUInt16(data, 0);
    }
}
