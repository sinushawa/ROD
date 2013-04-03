using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Graphics.Animation
{
    public class Pose
    {
        public string name;
        public Dictionary<int, ROD_core.Mathematics.DualQuaternion> localJoints;
    }
}
