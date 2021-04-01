using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class UIntConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(uint);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((uint)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToUInt32(data, 0);
    }
}
