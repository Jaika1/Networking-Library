﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Jaika1.Networking.Helpers.Conversion.Modules
{
    public class EnumConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(Enum);


        public byte[] ConvertToBytes(object instance, bool includeLength)
        {
            Enum e = (Enum)instance;
            Type underlyingType = e.GetType().GetEnumUnderlyingType();

            if (!ParentModule.HasConverterOfType(underlyingType))
                NetBase.WriteDebug($"Invalid element type for enum conversion! ({underlyingType})", true);

            List<byte> data = new List<byte>();

            data.AddRange(ParentModule.ConvertToBytes(Convert.ChangeType(e, underlyingType), includeLength));

            return data.ToArray();
        } 

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type)
        {
            Type underlyingType = type.GetEnumUnderlyingType();

            if (!ParentModule.HasConverterOfType(underlyingType))
                NetBase.WriteDebug($"Invalid element type for enum conversion! ({underlyingType})", true);

            return ParentModule.ObjectFromBytes(underlyingType, data, length);
        }
    }
}
