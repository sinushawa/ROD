using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{
    interface IAnimation
    {
        SequenceTiming time;
        float offset;
        float scale;

        AnimationType animationType;
        List<Model> targets;

        void UpdateTargets();
    }
}
