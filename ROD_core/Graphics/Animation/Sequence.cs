using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    public struct SequenceTiming
    {
        public float start;
        public float duration;
        public float end;
    }

    public class Sequence :IAnimation
    {
        public SequenceTiming time;

        public float offset = 0.0f;
        public float scale = 1.0f;
    }
}
