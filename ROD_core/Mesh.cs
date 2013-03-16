using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ROD_core
{
    [Serializable]
    public class Mesh : Component,ISerializable
    {
        public string name;
        private string filePath;
        public VertexStream _vertexStream;
        public IndexStream _indexStream;
        public Buffer vertexBuffer;
        public Buffer indexBuffer;

        public Mesh()
        {
            
        }

        protected Mesh(SerializationInfo info, StreamingContext context)
        {
            _vertexStream = (VertexStream)info.GetValue("vertexStream", typeof(VertexStream));
            _indexStream = (IndexStream)info.GetValue("indexStream", typeof(IndexStream));
            //var res= _vertexStream.getVertices();
        }

        public void Load(Device _device)
        {
            vertexBuffer = _vertexStream.createVerteBufferOnDevice(_device);
            indexBuffer = _indexStream.createIndexBufferOnDevice(_device);
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("vertexStream", _vertexStream, typeof(VertexStream));
            info.AddValue("indexStream", _indexStream, typeof(IndexStream));
        }

        public static Mesh createFromFile(string _filename)
        {
            
            BinaryFormatter bf = new BinaryFormatter();
            FileStream readStream = new FileStream(_filename, FileMode.Open);
            Mesh loadedMesh = (Mesh)bf.Deserialize(readStream);
            readStream.Close();
            loadedMesh.name = System.IO.Path.GetFileNameWithoutExtension(_filename);
            return loadedMesh;
        }
        public static void saveToFile(Mesh mesh, string _filename)
        {
            Stream stream = File.Open(_filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, mesh);
            stream.Close();
        }
        public void Dispose()
        {
            ToDispose(vertexBuffer);
            ToDispose(indexBuffer);
            ToDispose(_indexStream);
            ToDispose(_vertexStream);
        }
    }
}
