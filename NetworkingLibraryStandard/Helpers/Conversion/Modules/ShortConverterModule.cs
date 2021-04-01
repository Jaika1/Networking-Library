using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class ShortConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(short);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((short)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToInt16(data, 0);
    }
}
