using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ROD_core.Mathematics;

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

        public AnimationSkinningState animation;

        public void Update(float _delta)
        {
            currentPose = animation.ComputeLocalPose(_delta);
        }
        public List<DualQuaternion> GetJointWTMList()
        {
            return currentPose.GetWorldTransformVersion().GetJoints(TreeNavigation.Breadth_first).Select(x=> x.localRotationTranslation).ToList();
        }
    }
}
