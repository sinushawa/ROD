using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{
    public struct ClipTiming
    {
        public TimeSpan duration;
        public TimeSpan startOffset;
        public TimeSpan endOffset;
    }

    public abstract class Clip
    {
        public event EventHandler ClipFinished;

        private TimeSpan _actualLocalTime;
        private ClipTiming _time;
        private float _scale = 1.0f;
        private bool _isLooping = false;
        private AnimationType _animationType;
        private Model _target;

        public Clip()
        {
        }

        public TimeSpan actualLocalTime
        {
            get
            {
                return _actualLocalTime;
            }
            set
            {
                _actualLocalTime = (TimeSpan)value;
            }
        }
        public ClipTiming time
        {
            get
            {
                return _time;
            }
            set
            {
                _time=(ClipTiming)value;
            }
        }

        public float scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale=(float)value;
            }
        }
        public bool isLooping
        {
            get
            {
                return _isLooping;
            }
            set
            {
                _isLooping = (bool)value;
            }
        }

        public AnimationType animationType
        {
            get
            {
                return _animationType;
            }
            set
            {
                _animationType=(AnimationType)value;
            }
        }

        public Model target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = (Model)value;
            }
        }

        public abstract void Update(long _delta);
    }
}
