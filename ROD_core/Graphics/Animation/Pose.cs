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
        public List<Joint> joints;
        public bool isBindPose;

        public Pose(string _name)
            : this(_name, new List<Joint>(), false)
        {
        }

        public Pose(string _name, List<Joint> _root)
            : this(_name, _root, false)
        {
        }

        public Pose(string _name, List<Joint> _root, bool _isBindPose)
        {
            name = _name;
            joints = _root;
            isBindPose = _isBindPose;
        }

        protected Pose(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            joints = (List<Joint>)info.GetValue("joints", typeof(List<Joint>));
            isBindPose = (bool)info.GetValue("isBindPose", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("joints", joints, typeof(List<Joint>));
            info.AddValue("isBindPose", isBindPose, typeof(bool));
        }
        public Joint GetJointByName(string name)
        {
            Joint joint = joints.First(x => x.name == name);
            return joint;
        }
        public Joint GetJointById(int id)
        {
            Joint joint = joints.First(x => x.id == id);
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

        public Pose Clone (string _newName)
        {
            List<Joint> clonedJoints = new List<Joint>();
            foreach (Joint _joint in joints)
            {
                clonedJoints.Add(_joint.Clone());
            }
            Pose clone = new Pose(_newName, clonedJoints);
            return clone;
        }
        public Joint GetParent(Joint _joint)
        {
            Joint _parentJoint = null;
            if (_joint.parentId != -1)
            {
                _parentJoint = joints.First(x => x.id == _joint.parentId);
            }
            return _parentJoint;
        }
        private List<Joint> GetJointToRoot(Joint _joint)
        {
            List<Joint> _hierarchy = new List<Joint>();
            _hierarchy.Add(_joint);
            while (_joint.parentId != -1)
            {
                Joint _parentJoint = GetParent(_joint);
                _hierarchy.Add(_parentJoint);
                _joint = _parentJoint;
            }
            return _hierarchy;
        }
        private List<Joint> GetRootToJoint(Joint _joint)
        {
            List<Joint> _rootToJoint = GetJointToRoot(_joint);
            _rootToJoint.Reverse();
            return _rootToJoint;
        }
        private void ComputeWorldRotationTranslation(Joint _joint)
        {
            List<Joint> _hierarchy = GetJointToRoot(_joint);
            DualQuaternion _worldRotationTranslation = _hierarchy[0].localRotationTranslation;
            for (int i = 1; i < _hierarchy.Count; i++)
            {
                _worldRotationTranslation =_worldRotationTranslation * _hierarchy[i].localRotationTranslation;
            }
            _joint.worldRotationTranslation = _worldRotationTranslation;
        }
        public void ComputeWorldRotationTranslation()
        {
            foreach (Joint _joint in joints)
            {
                ComputeWorldRotationTranslation(_joint);
            }
        }
    }
}
