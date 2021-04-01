using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class ByteConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(byte);


        public byte[] ConvertToBytes(object instance, bool includeLength) => new[] { (byte)instance };

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (data[0], 1);
    }
}
