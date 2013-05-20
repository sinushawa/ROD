using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DualQuaternion = ROD_core.Mathematics.DualQuaternion;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ROD_core.Graphics.Animation
{
    [Serializable]
    public class Pose : ISerializable
    {
        public string name;
        public bool isBindPose;
        public Joint rootJoint;

        protected Pose(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            rootJoint = (Joint)info.GetValue("rootJoint", typeof(Joint));
            isBindPose = (bool)info.GetValue("isBindPose", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("rootJoint", rootJoint, typeof(Joint));
            info.AddValue("isBindPose", isBindPose, typeof(bool));
        }

        public List<Joint> GetAllJoints()
        {
            List<Joint> _joints = rootJoint.GetDepthEnumerable().ToList<Joint>();
            return _joints;
        }
        public Joint GetJointByName(string name)
        {
            Joint joint = GetAllJoints().Where(x => x.name == name).First();
            return joint;
        }
        public static void saveToFile(Joint joint, string _filename)
        {
            Stream stream = File.Open(_filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, joint);
            stream.Close();
        }
    }
}
