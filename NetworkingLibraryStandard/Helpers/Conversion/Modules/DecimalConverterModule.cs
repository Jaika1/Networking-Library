using System;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class DecimalConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(decimal);


        public byte[] ConvertToBytes(object instance, bool includeLength)
        {
            byte[] data = new byte[16];
            decimal d = (decimal)instance;
            int[] bits = decimal.GetBits(d);

            for(int i = 0; i < bits.Length; ++i)
            {
                data[(4 * i) + 0] = (byte)((bits[i] >> 24) & 0x000000FF);
                data[(4 * i) + 1] = (byte)((bits[i] >> 16) & 0x000000FF);
                data[(4 * i) + 2] = (byte)((bits[i] >> 8) & 0x000000FF);
                data[(4 * i) + 3] = (byte)((bits[i] >> 0) & 0x000000FF);
            }

            return data;
        } 

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type)
        {
            int[] bits = new int[4];

            for (int i = 0; i < bits.Length; ++i)
            {
                bits[i] = (data[(i * 4) + 0] << 24) | (data[(i * 4) + 1] << 16) | (data[(i * 4) + 2] << 8) | (data[(i * 4) + 3] << 0);
            }

            return (new decimal(bits), 16);
        }
    }
}
