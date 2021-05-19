using Jaika1.Networking.Helpers.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Jaika1.Networking
{
    public class DynamicPacket
    {
        private List<object> dataCollection = new List<object>();


        public DynamicPacket() { }

        public DynamicPacket(params object[] data)
        {
            foreach (object d in data)
                AddData(d);
        }


        internal byte[] GetRawData(ByteConverter converter)
        {
            if (dataCollection.Any(x => !converter.HasConverterOfType(x.GetType())))
                NetBase.WriteDebug($"The given converter couldn't parse one of the types of data provided!{Environment.NewLine}{string.Join(Environment.NewLine, dataCollection.Select(x => x.GetType()))}", true);

            if (dataCollection.Count == 0)
                return new byte[0];

            if (dataCollection.Count == 1)
                return converter.ConvertToBytes(dataCollection[0], false);

            // dataCollection.Count > 1
            List<byte> data = new List<byte>();
            dataCollection.ForEach(o => data.AddRange(converter.ConvertToBytes(o)));
            return data.ToArray();
        }

        public void AddData(object data) => dataCollection.Add(data);

        public T GetDataAt<T>(int index) => (T)dataCollection[index];

        [Obsolete("This method of data conversion poses a security risk and should no longer be used. See NetworkingLibrary.Helpers.Conversion.ByteConverter instead.", true)]
        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        [Obsolete("This method of data conversion poses a security risk and should no longer be used. See NetworkingLibrary.Helpers.Conversion.ByteConverter instead.", true)]
        public static object ByteArrayToObject(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object o = bf.Deserialize(ms);
                return o;
            }
        }

        internal static object[] GetInstancesFromData(byte[] data, ByteConverter converter, params Type[] types)
        {
            if (types.Any(x => !converter.HasConverterOfType(x)))
                NetBase.WriteDebug($"The given converter couldn't parse one of the provided types!{Environment.NewLine}{string.Join(Environment.NewLine, types.AsEnumerable())}", true);

            if (types.Count() == 0)
                return new object[0];

            if (types.Count() == 1)
                return new[] { converter.ObjectFromBytes(types[0], data).Instance };

            // types.Count() > 1

            object[] objects = new object[types.Count()];
            byte[] dataClone = data.Clone() as byte[];

            for(int i = 0; i < objects.Length; ++i)
            {
                (object Instance, int BytesParsed) t = converter.ObjectFromBytes(types[i], dataClone, 2);
                objects[i] = t.Instance;
                dataClone = dataClone.Skip(t.BytesParsed).ToArray();
            }

            return objects;
        }
    }
}
