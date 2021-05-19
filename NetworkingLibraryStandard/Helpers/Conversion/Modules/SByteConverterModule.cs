using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class SByteConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(sbyte);


        public byte[] ConvertToBytes(object instance, bool includeLength) => new[] { (byte)(0x00 | (sbyte)instance) };

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => ((sbyte)(0x00 | data[0]), 1);
    }
}
