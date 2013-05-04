using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    public class Clip_Skinning : Clip
    {


        public Queue<Pose> sequencesData;
        // End time of the coresponding sequences
        public Queue<TimeSpan> sequencesTiming;

        public override void Update(float _delta)
        {
        }
    }
}
