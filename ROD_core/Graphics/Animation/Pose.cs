using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DualQuaternion = ROD_core.Mathematics.DualQuaternion;
using System.IO;

namespace ROD_core.Graphics.Animation
{
    [Serializable]
    public class Pose : ISerializable
    {
        public string name;
        public Joint rootJoint;
        public bool isBindPose;

        public Pose(string _name, Joint _root)
            : this(_name, _root, false)
        {
        }

        public Pose(string _name, Joint _root, bool _isBindPose)
        {
            name = _name;
            rootJoint = _root;
            isBindPose = _isBindPose;
        }

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

        public List<Joint> GetJoints(TreeNavigation navigation)
        {
            List<Joint> _joints = rootJoint.GetEnumerable(navigation).ToList<Joint>();
            return _joints;
        }
        public Joint GetJointByName(string name)
        {
            Joint joint = GetJoints(TreeNavigation.depth_first).Where(x => x.name == name).First();
            return joint;
        }
        public Joint GetJointById(int id)
        {
            Joint joint = GetJoints(TreeNavigation.depth_first).Where(x => x.id == id).First();
            return joint;
        }
        public static Pose DLB(List<Pose> poses, List<float> weights)
        {
            return null;
        }
        public static Pose createFromFile(string _filename)
        {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream readStream = new FileStream(_filename, FileMode.Open);
            Pose loadedPose = (Pose)bf.Deserialize(readStream);
            readStream.Close();
            return loadedPose;
        }
        public void saveToFile(string _filename)
        {
            Stream stream = File.Open(_filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
        }
        public MemoryStream saveToMemory()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
            return stream;
        }
        public static void saveToFile(Pose _pose, string _filename)
        {
            _pose.saveToFile(_filename);
        }

        public Pose Clone ()
        {
            Joint clonedRoot = rootJoint.Clone(null);
            Pose clone = new Pose(this.name, clonedRoot);
            return clone;
        }
        public Pose GetWorldTransformVersion()
        {
            Pose worldPose = new Pose(this.name, null);
            worldPose.rootJoint = this.rootJoint.GetWorldTransformVersion();
            return worldPose;
        }
    }
}
