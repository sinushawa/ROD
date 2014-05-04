using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;

namespace ROD_core.Graphics.Animation
{
    public static class AssimpSkeleton
    {
        public static HierarchicalJoint ConstructSkeleton(Node[] _nodes, HierarchicalJoint _parent, List<Joint> _joints)
        {
            HierarchicalJoint _hJoint= new HierarchicalJoint("", _parent);
            foreach (Node _node in _nodes)
            {
                if (!_node.HasMeshes)
                {
                    _hJoint.name = _node.Name;
                    Joint _corresponding = _joints.FirstOrDefault(x => x.name == _node.Name);
                    if (_corresponding != null)
                    {
                        _hJoint.localRotationTranslation = _corresponding.localRotationTranslation;
                        _hJoint.worldRotationTranslation = _corresponding.worldRotationTranslation;
                        if (_node.Children != null)
                        {
                            _hJoint.children.Add(ConstructSkeleton(_node.Children, _hJoint, _joints));
                        }
                    }
                    else
                    {
                        if (_node.Children != null)
                        {
                            _hJoint = ConstructSkeleton(_node.Children, null, _joints);
                        }
                    }
                }
            }
            return _hJoint;
        }
    }
}
