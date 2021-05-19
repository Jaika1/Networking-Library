using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class UIntConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(uint);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((uint)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToUInt32(data, 0), 4);
    }
}
