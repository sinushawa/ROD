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
    public class HierarchicalJoint : ISerializable
    {
        public string name;
        public HierarchicalJoint parent;
        public List<HierarchicalJoint> children;
        public DualQuaternion worldRotationTranslation;
        public DualQuaternion localRotationTranslation;

        public HierarchicalJoint(string _name, HierarchicalJoint _parent) : this(_name, _parent, DualQuaternion.Identity, DualQuaternion.Identity)
        {
        }
        public HierarchicalJoint(string _name, HierarchicalJoint _parent, DualQuaternion _worldRotationTranslation, DualQuaternion _localRotationTranslation)
        {
            name = _name;
            parent = _parent;
            children = new List<HierarchicalJoint>();
            worldRotationTranslation = _worldRotationTranslation;
            localRotationTranslation = _localRotationTranslation;
        }

        protected HierarchicalJoint(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            children = (List<HierarchicalJoint>)info.GetValue("children", typeof(List<HierarchicalJoint>));
            worldRotationTranslation = (DualQuaternion)info.GetValue("worldRotationTranslation", typeof(DualQuaternion));
            localRotationTranslation = (DualQuaternion)info.GetValue("localRotationTranslation", typeof(DualQuaternion));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("children", children, typeof(List<HierarchicalJoint>));
            info.AddValue("worldRotationTranslation", worldRotationTranslation, typeof(DualQuaternion));
            info.AddValue("localRotationTranslation", localRotationTranslation, typeof(DualQuaternion));
        }
        public HierarchicalJoint Clone()
        {
            HierarchicalJoint clone = new HierarchicalJoint(name, null);
            return clone;
        }
    }
}
