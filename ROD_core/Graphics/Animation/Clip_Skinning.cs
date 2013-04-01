using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    public class Poses
    {
        protected Pose poseA;
        protected Pose poseB;
    }
    public struct PoseTime
    {
        public TimeSpan startTime;
        public TimeSpan endTime;
    }

    public class Clip_Skinning : Clip
    {
        public Dictionary<PoseTime, Poses> animationData;

        public override void Update(float _delta)
        {
            actualLocalTime = actualLocalTime + TimeSpan.FromMilliseconds(_delta);
            KeyValuePair<PoseTime, Poses> poses=animationData.Where(x => x.Key.startTime < actualLocalTime && x.Key.endTime > actualLocalTime).First();
            TimeSpan duration = poses.Key.endTime - poses.Key.startTime;
            TimeSpan positionTimeline = actualLocalTime - poses.Key.startTime;
            float weight = (float)(positionTimeline.TotalMilliseconds/duration.TotalMilliseconds);
        }
    }
}
