using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Assets;

namespace ROD_core.Graphics.Animation
{

    public abstract class Clip
    {
        public event EventHandler ClipFinished;
        private TimeSpan _localTime = new TimeSpan(0);
        private float _scale = 1.0f;
        public bool isPlaying = false;
        private bool _isLooping = false;
        private AnimationType _animationType;

        public Clip()
        {
        }

        public TimeSpan localTime
        {
            get
            {
                return _localTime;
            }
            set
            {
                _localTime = (TimeSpan)value;
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

        public abstract void Update(float _delta);
        public virtual void Start()
        {
            isPlaying = true;
        }
    }
}
