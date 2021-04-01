using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class SByteConverterModule : IByteConverterModule
    {
        public Type T { get; } = typeof(sbyte);


        public byte[] ConvertToBytes(object instance, bool includeLength) => new[] { (byte)(0x00 | (sbyte)instance) };

        public object ObjectFromBytes(byte[] data, int length) => (sbyte)(0x00 | data[0]);
    }
}
