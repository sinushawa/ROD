using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{

    public class AnimationTrack
    {
        public int id;
        public string name;
        public Dictionary<TimeSpan,Sequence> sequences;

        public AnimationTrack(int _id)
        {
            id = _id;
        }
    }
}
