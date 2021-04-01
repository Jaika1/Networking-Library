﻿using System;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class ByteConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(byte);


        public byte[] ConvertToBytes(object instance, bool includeLength) => new[] { (byte)instance };

        public object ObjectFromBytes(byte[] data, int length) => data[0];
    }
}
