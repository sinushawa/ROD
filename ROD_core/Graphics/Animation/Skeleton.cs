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
    public class Skeleton
    {
        public string name;
        public Guid id;
        public SkeletonArchetype skeletonType;
        public int jointCount;
        public Pose bindPose;
        public Pose currentPose;
    }
}
