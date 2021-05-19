using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class BooleanConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(bool);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((bool)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToBoolean(data, 0), 1);
    }
}
