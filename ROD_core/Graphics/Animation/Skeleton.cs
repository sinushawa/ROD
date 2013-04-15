using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    [Flags]
    public enum SkeletonArchetype
    {
        None = (1 << 0),
        Biped = (1 << 1),
        Quadruped = (1 << 1)
    }
    public class Bone
    {
        string name;

    public class Skeleton
    {
        public string name;
        public SkeletonArchetype skeletonType;
        public List<string> description;
    }
}
