using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class FloatConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(float);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((float)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToSingle(data, 0), 4);
    }
}
