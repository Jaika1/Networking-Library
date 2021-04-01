using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class BooleanConverterModule : IByteConverterModule
    {
        public Type T { get; } = typeof(bool);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((bool)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToBoolean(data, 0);
    }
}
