using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{
    public struct SequenceTiming
    {
        public TimeSpan duration;
        public TimeSpan startOffset;
        public TimeSpan endOffset;
    }

    public class Sequence :IAnimation
    {
        private SequenceTiming _time;

        private float _scale = 1.0f;
        private AnimationType _animationType;
        private List<Model> _targets;

        public Sequence()
        {
        }

        SequenceTiming time
        {
            get
            {
                return _time;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        float scale
        {
            get
            {
                return _scale;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        AnimationType animationType
        {
            get
            {
                return _animationType;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        List<Model> targets
        {
            get
            {
                return _targets;
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
