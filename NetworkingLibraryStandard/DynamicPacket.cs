using NetworkingLibrary.Helpers.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkingLibrary
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
            #region old method
            //List<byte> dataArray = new List<byte>();

            //if (dataCollection.Count == 1)
            //{
            //    dataArray.AddRange(ObjectToByteArray(dataCollection[0]));
            //}
            //else
            //{
            //    for (int i = 0; i < dataCollection.Count; ++i)
            //    {
            //        object o = dataCollection[i];
            //        byte[] objectData = ObjectToByteArray(o);
            //        if (objectData.Length > ushort.MaxValue) 
            //            throw new Exception($"One object contained in this dynamic packet at position {i} takes up more than {ushort.MaxValue} bytes, which is absuredly large for any packet to begin with. Consider splitting this data into more managable segments manually, or optimise by only sending nessecary data from the object. (Object type is {o.GetType().FullName})");
            //        ushort dataLength = (ushort)objectData.Length;
            //        byte[] addData = new byte[2 + dataLength];
            //        BitConverter.GetBytes(dataLength).CopyTo(addData, 0);
            //        objectData.CopyTo(addData, 2);
            //        dataArray.AddRange(addData);
            //    }
            //}

            //return dataArray.ToArray();
            #endregion

            if (dataCollection.Any(x => !converter.HasConverterOfType(x.GetType())))
                throw new Exception("The given converter couldn't parse one of the types of data provided!");

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
            if (types.Any(x => !converter.HasConverterOfType(x.GetType())))
                throw new Exception("The given converter couldn't parse one of the provided types!");


        }
    }
}
