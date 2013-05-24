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
        public Joint parent;
        public List<Joint> children;
        public DualQuaternion localRotationTranslation;

        public Joint(int _id, string _name, object _parent, DualQuaternion _localRotationTranslation)
        {
            id = _id;
            name = _name;
            if (_parent != null)
            {
                parent = (Joint)_parent;
            }
            children = new List<Joint>();
            localRotationTranslation = _localRotationTranslation;
        }

        protected Joint(SerializationInfo info, StreamingContext context)
        {
            id = (int)info.GetValue("id", typeof(int));
            name = (string)info.GetValue("name", typeof(string));
            parent = (Joint)info.GetValue("parent", typeof(Joint));
            children = (List<Joint>)info.GetValue("children", typeof(List<Joint>));
            localRotationTranslation = (DualQuaternion)info.GetValue("localRotationTranslation", typeof(DualQuaternion));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id, typeof(int));
            info.AddValue("name", name, typeof(string));
            info.AddValue("parent", parent, typeof(Joint));
            info.AddValue("children", children, typeof(List<Joint>));
            info.AddValue("localRotationTranslation", localRotationTranslation, typeof(DualQuaternion));
        }
        public IEnumerable<Joint> GetDepthEnumerable()
        {
            var queue = new Queue<Joint>();
            queue.Enqueue(this);

            while (0 < queue.Count)
            {
                Joint node = queue.Dequeue();

                foreach (Joint child in node.children)
                {
                    queue.Enqueue(child);
                }

                yield return node;
            }
        }
        private void Queuing(Joint joint, Queue<Joint> stack)
        {
            stack.Enqueue(joint);
            if (joint.parent != null)
            {
                Queuing(joint.parent, stack);
            }
        }
        public IEnumerable<Joint> GetParentEnumerable()
        {
            Queue<Joint> stack = new Queue<Joint>();
            Queuing(this, stack);
            while (stack.Count > 0)
            {
                yield return stack.Dequeue();
            }
        }
        public DualQuaternion GetWorldTransform()
        {
            DualQuaternion parentTransform = parent.GetWorldTransform();
            DualQuaternion jointWorldTransform = localRotationTranslation * parentTransform;
            return jointWorldTransform;
        }
    }
}
