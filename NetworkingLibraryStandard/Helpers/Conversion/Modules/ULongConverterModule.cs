using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class ULongConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(ulong);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((ulong)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToUInt64(data, 0);
    }
}
