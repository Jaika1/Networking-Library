using System;

namespace NetworkingLibrary.Helpers.Conversion
{
    public interface IByteConverterModule
    {
        Type T { get; }

        byte[] ConvertToBytes(object instance, bool includeLength);

        object ObjectFromBytes(byte[] data, int length);
    }
}
