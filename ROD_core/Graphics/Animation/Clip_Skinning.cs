using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    public class PoseInterpolation
    {
        protected Pose poseA;
        protected Pose poseB;
        protected float weight;
    }

    public class Clip_Skinning : Clip
    {
        public Dictionary<TimeSpan, Pose> animationData;

        public override void Update(long _delta)
        {
            actualLocalTime = actualLocalTime + new TimeSpan(_delta);
            animationData.FindPreviousItem<KeyValuePair<TimeSpan, Pose>>(x=> x.Key< actualLocalTime).First();
        }
    }
}
