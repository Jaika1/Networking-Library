﻿using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class DoubleConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(double);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((double)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToDouble(data, 0);
    }
}
