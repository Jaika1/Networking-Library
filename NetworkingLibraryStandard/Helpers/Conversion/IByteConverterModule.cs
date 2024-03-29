﻿using System;

namespace Jaika1.Networking.Helpers.Conversion
{
    public interface IByteConverterModule
    {
        ByteConverter ParentModule { get; set; }

        Type T { get; }

        byte[] ConvertToBytes(object instance, bool includeLength);

        (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type instanceType);
    }
}
