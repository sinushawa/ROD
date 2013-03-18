using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

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
        private SequenceTiming _time;

        private float _offset = 0.0f;
        private float _scale = 1.0f;
        private AnimationType _animationType;
        private List<Model> _targets;

        SequenceTiming time
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        float IAnimation.offset
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        float IAnimation.scale
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        AnimationType IAnimation.animationType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        List<Model> IAnimation.targets
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void UpdateTargets()
        {
            throw new NotImplementedException();
        }
    }
}
