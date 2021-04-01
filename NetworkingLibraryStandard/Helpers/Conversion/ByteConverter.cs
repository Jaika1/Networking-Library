﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkingLibrary.Helpers.Conversion
{
    public class ByteConverter
    {
        private List<IByteConverterModule> knownConverters = new List<IByteConverterModule>();


        public IReadOnlyList<IByteConverterModule> ConverterModules => knownConverters;


        public ByteConverter(bool includeDefaults = true)
        {
            if (includeDefaults)
            {
                Type[] converters = (from t in Assembly.GetExecutingAssembly().GetTypes()
                                     where t.GetInterfaces().Contains(typeof(IByteConverterModule))
                                     select t).ToArray();

                foreach (Type c in converters)
                    AddConverter((IByteConverterModule)Activator.CreateInstance(c));
            }
        }


        public bool HasConverterOfType(Type type) => knownConverters.Any(c => c.T == type);

        public bool HasConverterOfType<T>() => knownConverters.Any(c => c.T == typeof(T));

        public void AddConverter(IByteConverterModule converter)
        {
            if (knownConverters.Any(c => c.T == converter.T))
            {
                throw new Exception($"A converter for type {converter.T.FullName} has already been added!");
            }

            converter.ParentModule = this;
            knownConverters.Add(converter);
        }

        public byte[] ConvertToBytes(object instance, bool includeLength = true)
        {
            Type instanceType = instance.GetType();

            if (!knownConverters.Any(c => c.T.Equals(instanceType)))
                throw new Exception($"No conversion method exists for type of {instanceType.FullName}!");

            return knownConverters.First(c => c.T.Equals(instanceType)).ConvertToBytes(instance, includeLength);
        }

        public (T Instance, int BytesParsed) ObjectFromBytes<T>(byte[] data, int length = -1)
        {
            (object, int) t = ObjectFromBytes(typeof(T), data, length);
            return ((T)t.Item1, t.Item2);
        }

        public (object Instance, int BytesParsed) ObjectFromBytes(Type instanceType, byte[] data, int length = -1)
        {
            if (!knownConverters.Any(c => c.T == instanceType))
                throw new Exception($"No conversion method exists for type of {instanceType.FullName}!");

            return knownConverters.First(c => c.T == instanceType).ObjectFromBytes(data, length);
        }
    }
}
