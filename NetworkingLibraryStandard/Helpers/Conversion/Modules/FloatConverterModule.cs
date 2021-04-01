﻿using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class FloatConverterModule : IByteConverterModule
    {
        public Type T { get; } = typeof(float);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((float)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToSingle(data, 0);
    }
}
