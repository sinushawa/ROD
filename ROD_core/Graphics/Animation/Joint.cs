using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Graphics.Animation
{
    // this model only take bones rotation into account.
    public class Joint
    {
        public int id;
        public Joint parent;
        public List<Joint> children;

        // this would be the original position of the joint before transformation.
        public Vector3 position;
        public Quaternion localRotation;
        public Matrix globalTransformation;
    }
}
