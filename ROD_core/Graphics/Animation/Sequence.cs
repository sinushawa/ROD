using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core.Graphics.Animation
{
    public class WeightedClip
    {
        public float weight;
        public Clip clip;

        public WeightedClip(Clip _clip):this(_clip, 1.0f)
        {
        }
        public WeightedClip(Clip _clip, float _weight)
        {
            clip = _clip;
            weight = _weight;
        }
    }

    public class Sequence
    {
        private List<WeightedClip> _weightedClips;

        public Sequence()
        {
        }

        public bool AddClip(WeightedClip _weightedClip)
        {
            if (_weightedClips.Count == 0)
            {
                _weightedClips.Add(_weightedClip);
                return true;
            }
            else if (_weightedClips.Count > 0 && _weightedClip.clip.target == _weightedClips[0].clip.target)
            {
                _weightedClips.Add(_weightedClip);
                NormalizeWeights();
                return true;
            }
            else
            {
                return false;
            }
        }
        private void NormalizeWeights()
        {
            float weightsTotal = _weightedClips.Sum(x => x.weight);
            for (int i = 0; i < _weightedClips.Count; i++)
            {
                _weightedClips[i].weight = (_weightedClips[i].weight / weightsTotal);
            }
        }
    }
}
