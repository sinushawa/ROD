using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.Serialization;
using System.Reflection;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ROD_core
{
    [Serializable]
    public class VertexStream : ISerializable, IDisposable
    {
        public DataStream dataStream;
        internal Type vertexType;
        public Semantic vertexDefinition;
        MethodInfo readMethod;
        MethodInfo writeMethod;
        IList list;
        byte[] buffer;
        private Buffer vertexBuffer;
        private BufferDescription bufferDescription;

        public VertexStream(int _vertexCount, bool canRead, bool canWrite, Semantic _vertexDefinition)
        {
            vertexDefinition = _vertexDefinition.reorderDefinition();
            vertexType = DynamicVertex.CreateVertex(vertexDefinition);
            dataStream = new DataStream(vertexType.SizeOf()*_vertexCount, canRead, canWrite);

            createReadWriteMethods();
        }
        protected VertexStream(SerializationInfo info, StreamingContext context)
        {
            vertexDefinition = (Semantic)info.GetValue("vertexDefinition", typeof(Semantic));
            vertexType = DynamicVertex.CreateVertex(vertexDefinition);
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
            readMethod = readMethod.MakeGenericMethod(vertexType);

            // Retrieve DataStream Write method implementing a generic format taking the vertex Type
            writeMethod = typeof(DataStream).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where<MethodInfo>(m => m.IsGenericMethod && m.Name == "Write").FirstOrDefault();
            // Build a method with the specific type argument you're interested in
            writeMethod = writeMethod.MakeGenericMethod(vertexType);
        }

        public IList getVertices()
        {
            dataStream.Position = 0;
            list = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(vertexType)));
            var val = vertexType.SizeOf();
            while (dataStream.Position < dataStream.Length)
            {
                var tip = readMethod.Invoke(dataStream, new object[] { });
                list.Add(readMethod.Invoke(dataStream, new object[] { }));
            }
            dataStream.Position = 0;
            return list;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dataStream.Position = 0;
            buffer = new byte[dataStream.Length];
            dataStream.Read(buffer, 0, (int)dataStream.Length);
            info.AddValue("vertexDefinition", vertexDefinition, typeof(Semantic));
            info.AddValue("buffer", buffer, typeof(byte[]));
        }
        public void WriteVertex(object[] _vertex)
        {
            object unFormedVertex = Activator.CreateInstance(vertexType, _vertex);
            Convert.ChangeType(unFormedVertex, vertexType);
            writeMethod.Invoke(dataStream, new object[] { unFormedVertex });
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

        public Buffer createVerteBufferOnDevice(Device _device)
        {
            // need to make a statement to check if defined
            bufferDescription = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (int)dataStream.Length,
                Usage = ResourceUsage.Default,
            };
            dataStream.Position = 0;
            Buffer vertices = new Buffer(_device, dataStream, bufferDescription);
            dataStream.Close();
            return vertices;
        }

        public void Dispose()
        {
            vertexBuffer.Dispose();
            dataStream.Dispose();
        }
    }
}
