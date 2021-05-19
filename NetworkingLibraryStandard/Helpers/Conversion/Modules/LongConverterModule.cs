using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class LongConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(long);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((long)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToInt64(data, 0), 8);
    }
}
