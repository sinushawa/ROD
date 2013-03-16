using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.Serialization;
using System.Reflection;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.ComponentModel;

namespace ROD_core
{
    [Serializable]
    public class IndexStream : ISerializable, IDisposable
    {
        public DataStream dataStream;
        MethodInfo readMethod;
        MethodInfo writeMethod;
        private Type countType;
        byte[] buffer;
        private Buffer indexBuffer;
        private BufferDescription bufferDescription;


        public IndexStream(int _indexCount, Type _countType, bool canRead, bool canWrite)
        {
            countType = _countType;
            dataStream = new DataStream(countType.SizeOf() * _indexCount, canRead, canWrite);
            createReadWriteMethods();
        }
        protected IndexStream(SerializationInfo info, StreamingContext context)
        {
            countType = (Type)info.GetValue("countType", typeof(Type));
            buffer = (byte[])info.GetValue("buffer", typeof(byte[]));
            dataStream = new DataStream(buffer.Length, true, true);
            dataStream.Write(buffer, 0, buffer.Length);
            createReadWriteMethods();
        }

        private void createReadWriteMethods()
        {
            // Retrieve DataStream Read method implementing a generic format taking the vertex Type
            readMethod = typeof(DataStream).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where<MethodInfo>(m => m.IsGenericMethod && m.Name == "Read").FirstOrDefault();
            // Build a method with the specific type argument you're interested in
            readMethod = readMethod.MakeGenericMethod(countType);

            // Retrieve DataStream Write method implementing a generic format taking the vertex Type
            writeMethod = typeof(DataStream).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where<MethodInfo>(m => m.IsGenericMethod && m.Name == "Write").FirstOrDefault();
            // Build a method with the specific type argument you're interested in
            writeMethod = writeMethod.MakeGenericMethod(countType);
        }

        public void WriteIndex(object _index)
        {
            writeMethod.Invoke(dataStream, new object[] { _index });
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dataStream.Position = 0;
            buffer = new byte[dataStream.Length];
            dataStream.Read(buffer, 0, (int)dataStream.Length);
            info.AddValue("countType", countType, typeof(Type));
            info.AddValue("buffer", buffer, typeof(byte[]));
        }

        public BufferDescription BufferDescription
        {
            get
            {
                return bufferDescription;
            }
            set
            {
                bufferDescription = value;
            }
        }

        public Buffer createIndexBufferOnDevice(Device _device)
        {
            // need to make a statement to check if defined
            bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)dataStream.Length,
                Usage = ResourceUsage.Default,
            };
            dataStream.Position = 0;
            Buffer indices = new Buffer(_device, dataStream, bufferDescription);
            dataStream.Close();
            return indices;
        }
        public int getIndexCount()
        {
            return (int)(dataStream.Length / countType.SizeOf());
        }

        public void Dispose()
        {
            dataStream.Dispose();
            indexBuffer.Dispose();
        }
    }
}
