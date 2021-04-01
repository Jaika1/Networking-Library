using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class LongConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(long);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((long)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToInt64(data, 0);
    }
}
