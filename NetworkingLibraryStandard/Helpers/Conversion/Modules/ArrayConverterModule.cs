using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NetworkingLibrary.Helpers.Conversion.Modules
{
    public class ArrayConverterModule : IByteConverterModule
    {
        public ByteConverter ParentModule { get; set; } = null;

        public Type T { get; } = typeof(Array);


        public byte[] ConvertToBytes(object instance, bool includeLength)
        {
            Array a = (Array)instance;
            Type elementType = instance.GetType().GetElementType();

            if (!ParentModule.HasConverterOfType(elementType))
                throw new Exception("Invalid element type!");

            List<byte> data = new List<byte>();

            data.AddRange(ParentModule.ConvertToBytes((ushort)a.Length));

            for(int i = 0; i < a.Length; ++i)
                data.AddRange(ParentModule.ConvertToBytes(a.GetValue(i)));

            return data.ToArray();
        } 

        public (object Instance, int BytesParsed) ObjectFromBytes(byte[] data, int length, Type type)
        {
            Type elementType = type.GetElementType();

            if (!ParentModule.HasConverterOfType(elementType))
                throw new Exception("Invalid element type!");

            //The 'length' parameter here is used to determine how many bytes at the start of the array are for the length of the string data
            (ushort Instance, int BytesParsed) t = ParentModule.ObjectFromBytes<ushort>(data);

            ushort arraySize = t.Instance;

            byte[] arrayBytes = data.Skip(t.BytesParsed).ToArray();

            Array instances = Array.CreateInstance(elementType, arraySize);

            int parsed = t.BytesParsed;

            for (int i = 0; i < arraySize; ++i)
            {
                (object Instance, int BytesParsed) instanceTuple = ParentModule.ObjectFromBytes(elementType, arrayBytes, t.BytesParsed);
                instances.SetValue(instanceTuple.Instance, i);
                parsed += instanceTuple.BytesParsed;
                arrayBytes = arrayBytes.Skip(instanceTuple.BytesParsed).ToArray();
            }

            return (instances, parsed);
        }
    }
}
