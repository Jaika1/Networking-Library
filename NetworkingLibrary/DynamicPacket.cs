using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkingLibrary
{
    public class DynamicPacket
    {
        private List<object> dataCollection = new List<object>();


        public byte[] GetRawData()
        { 
            List<byte> dataArray = new List<byte>();

            if (dataCollection.Count == 1)
            {
                dataArray.AddRange(ObjectToByteArray(dataCollection[0]));
            }
            else
            {
                for (int i = 0; i < dataCollection.Count; ++i)
                {
                    object o = dataCollection[i];
                    byte[] objectData = ObjectToByteArray(o);
                    if (objectData.Length > ushort.MaxValue) 
                        throw new Exception($"One object contained in this dynamic packet at position {i} takes up more than {ushort.MaxValue} bytes, which is absuredly large for any packet to begin with. Consider splitting this data into more managable segments manually, or optimise by only sending nessecary data from the object. (Object type is {o.GetType().FullName})");
                    ushort dataLength = (ushort)objectData.Length;
                    byte[] addData = new byte[2 + dataLength];
                    BitConverter.GetBytes(dataLength).CopyTo(addData, 0);
                    objectData.CopyTo(addData, 2);
                    dataArray.AddRange(addData);
                }
            }

            return dataArray.ToArray();
        }

        public void AddData(object data) => dataCollection.Add(data);

        public T GetDataAt<T>(int index) => (T)dataCollection[index];

        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
