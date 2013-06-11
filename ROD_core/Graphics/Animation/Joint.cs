using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DualQuaternion = ROD_core.Mathematics.DualQuaternion;

namespace ROD_core.Graphics.Animation
{
    [Serializable]
    public class Joint : ISerializable
    {
        public int id;
        public string name;
        public int parentId;
        public DualQuaternion worldRotationTranslation;
        public DualQuaternion localRotationTranslation;

        public Joint(int _id, string _name)
            : this(_id, _name, -1, DualQuaternion.Identity, DualQuaternion.Identity)
        {
        }
        public Joint(int _id, string _name, int _parentId) : this( _id, _name, _parentId, DualQuaternion.Identity, DualQuaternion.Identity)
        {
        }
        public Joint(int _id, string _name, int _parentId, DualQuaternion _worldRotationTranslation, DualQuaternion _localRotationTranslation)
        {
            id = _id;
            name = _name;
            parentId = _parentId;
            worldRotationTranslation = _worldRotationTranslation;
            localRotationTranslation = _localRotationTranslation;
        }

        protected Joint(SerializationInfo info, StreamingContext context)
        {
            id = (int)info.GetValue("id", typeof(int));
            name = (string)info.GetValue("name", typeof(string));
            parentId = (int)info.GetValue("parentId", typeof(int));
            worldRotationTranslation = (DualQuaternion)info.GetValue("worldRotationTranslation", typeof(DualQuaternion));
            localRotationTranslation = (DualQuaternion)info.GetValue("localRotationTranslation", typeof(DualQuaternion));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id, typeof(int));
            info.AddValue("name", name, typeof(string));
            info.AddValue("parentId", parentId, typeof(int));
            info.AddValue("worldRotationTranslation", worldRotationTranslation, typeof(DualQuaternion));
            info.AddValue("localRotationTranslation", localRotationTranslation, typeof(DualQuaternion));
        }
        public Joint Clone()
        {
            Joint clone = new Joint(id, name, parentId);
            return clone;
        }
    }
}
