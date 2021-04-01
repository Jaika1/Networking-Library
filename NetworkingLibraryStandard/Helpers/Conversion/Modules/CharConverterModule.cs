﻿using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class CharConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(char);


        public byte[] ConvertToBytes(object instance, bool includeLength) => BitConverter.GetBytes((char)instance);

        public object ObjectFromBytes(byte[] data, int length) => BitConverter.ToChar(data, 0);
    }
}
