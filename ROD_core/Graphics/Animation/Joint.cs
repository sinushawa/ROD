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
        public DualQuaternion worldRotationTranslation;

        public Joint(int _id, string _name)
            : this(_id, _name, null, DualQuaternion.Identity)
        {
        }
        public Joint(int _id, string _name, object _parent) : this( _id, _name, _parent, DualQuaternion.Identity)
        {
        }
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
            worldRotationTranslation = (DualQuaternion)info.GetValue("worldRotationTranslation", typeof(DualQuaternion));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id, typeof(int));
            info.AddValue("name", name, typeof(string));
            info.AddValue("parent", parent, typeof(Joint));
            info.AddValue("children", children, typeof(List<Joint>));
            info.AddValue("localRotationTranslation", localRotationTranslation, typeof(DualQuaternion));
            info.AddValue("worldRotationTranslation", worldRotationTranslation, typeof(DualQuaternion));
        }
        public IEnumerable<Joint> GetEnumerable()
        {
            return GetEnumerable(TreeNavigation.depth_first);
        }
        public IEnumerable<Joint> GetEnumerable(TreeNavigation navigation)
        {
            if (navigation == TreeNavigation.depth_first)
            {
                return this.GetDepthEnumerable();
            }
            else
            {
                return this.GetBreadthEnumerable();
            }
        }
        private IEnumerable<Joint> GetDepthEnumerable()
        {
            yield return this;
            foreach (Joint child in children)
            {
                var e = child.GetDepthEnumerable().GetEnumerator();
                while (e.MoveNext())
                {
                    yield return e.Current;
                }
            }
            
            
        }
        private IEnumerable<Joint> GetBreadthEnumerable()
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
        public Joint Clone(Joint _parent)
        {
            Joint joint = new Joint(this.id, this.name, _parent);
            int childrensNb = this.children.Count;
            for (int i = 0; i < childrensNb; i++)
            {
                joint.children.Add(this.children[i].Clone(joint));
            }
            return joint;
        }
        
        public Joint GetWorldTransformVersion()
        {
            return AggreagateJoints(this, null);
        }
        private static ROD_core.Graphics.Animation.Joint AggreagateJoints(Joint joint, Joint _parent)
        {
            Joint TJoint = new Joint(joint.id, joint.name, _parent, joint.localRotationTranslation);
            DualQuaternion DQ = AggregateLocalTM(TJoint);
            TJoint.localRotationTranslation = DQ;
            int childrensNb = joint.children.Count;
            for (int i = 0; i < childrensNb; i++)
            {
                TJoint.children.Add(AggreagateJoints(joint.children[i], TJoint));
            }
            return TJoint;
        }
        private static DualQuaternion AggregateLocalTM(Joint joint)
        {
            DualQuaternion DQ = DualQuaternion.Identity;
            if (joint.parent != null)
            {
                DQ = joint.parent.localRotationTranslation;
            }
            DualQuaternion LDQ = joint.localRotationTranslation;
            DQ = LDQ * DQ;
            DQ.Normalize();
            return DQ;
        }
    }
}
