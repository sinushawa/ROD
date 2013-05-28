using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Mathematics;

namespace ROD_core.Graphics.Animation
{
    public class AnimationSkinningState
    {
        public List<Clip_Skinning> clips = new List<Clip_Skinning>();
        public List<float> clipWeights = new List<float>();

        public Pose ComputeLocalPose(float _delta)
        {
            clips.ForEach(x => x.Update(_delta));
            List<Pose> poses = clips.SelectMany(x => new List<Pose> { x.previousPose, x.nextPose }).ToList();
            List<float> sweights = clips.Select(x => x.nweight ).Zip(clipWeights, (x,y)=> x*y).ToList();
            List<float> weights = sweights.SelectMany(x => new List<float> { 1 - x, x }).ToList();

            List<List<Joint>> jointsPerPose = poses.Select(x => x.GetJoints(TreeNavigation.depth_first).ToList()).ToList();
            List<List<Joint>> jointsPerId = poses.SelectMany(x => x.GetJoints(TreeNavigation.depth_first)).GroupBy(x=> x.id).Select(x=> x.ToList()).ToList();
            Pose currentPose = poses[0].Clone();
            List<Joint> currentJoints = currentPose.GetJoints(TreeNavigation.depth_first);
            for (int i=0; i<jointsPerId.Count; i++)
            {
                List<DualQuaternion> DQs = jointsPerId[i].Select(x=> x.localRotationTranslation).ToList();
                currentJoints[i].localRotationTranslation = DualQuaternion.DLB(DQs, weights);
            }
            return currentPose;
        }
        public void AddClip(Clip_Skinning clip, float weight)
        {
            clips.Add(clip);
            clipWeights.Add(weight);
            NormalizeWeights();
        }
        private void NormalizeWeights()
        {
            float sum = clipWeights.Sum();
            clipWeights.ForEach(x => x = x / sum);
        }
    }
}
