using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class ShortConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(short);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((short)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToInt16(data, 0), 2);
    }
}
