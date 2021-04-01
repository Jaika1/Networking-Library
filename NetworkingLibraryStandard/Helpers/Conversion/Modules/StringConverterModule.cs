using System;
using System.Linq;
using System.Text;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class StringConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(string);

        public byte[] ConvertToBytes(object instance, bool includeLength)
        {
            string s = (string)instance;
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(s);

            if (!includeLength)
                return utf8Bytes;

            byte[] lengthBytes = ParentModule.ConvertToBytes((ushort)utf8Bytes.Length);
            byte[] data = new byte[lengthBytes.Length + utf8Bytes.Length];
            lengthBytes.CopyTo(data, 0);
            utf8Bytes.CopyTo(data, lengthBytes.Length);

            return data;
        }

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length)
        {
            if (length == -1)
                return (Encoding.UTF8.GetString(data), data.Length);

            //The 'length' parameter here is used to determine how many bytes at the start of the array are for the length of the string data
            byte[] lengthData = data.Take(length).ToArray();
            ushort dataLength = ParentModule.ObjectFromBytes<ushort>(lengthData).Instance;

            byte[] utf8Bytes = data.Skip(length).Take(dataLength).ToArray();

            return (Encoding.UTF8.GetString(utf8Bytes), lengthData.Length + utf8Bytes.Length);
        }
    }
}
