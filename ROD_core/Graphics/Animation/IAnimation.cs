using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{
    interface IAnimation
    {
        SequenceTiming time
        {
            get;
            set;
        }
        float scale
        {
            get;
            set;
        }

        AnimationType animationType
        {
            get;
            set;
        }
        List<Model> targets
        {
            get;
            set;
        }

        void UpdateTargets();
    }
}
