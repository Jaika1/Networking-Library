using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class IntConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(int);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((int)instance);

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type) => (BitConverter.ToInt32(data, 0), 4);
    }
}
