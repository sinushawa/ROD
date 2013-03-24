using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{
    interface IAnimation
    {
        event EventHandler ClipFinished;

        TimeSpan actualLocalTime
        {
            get;
            set;
        }
        ClipTiming time
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
        Model target
        {
            get;
            set;
        }

        void Update();
    }
}
