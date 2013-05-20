using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Mathematics;

namespace ROD_core.Graphics.Animation
{
    
    public class Clip_Skinning : Clip
    {

        public List<Pose> sequencesData;
        // End time of the coresponding sequences every timing is local (the time correspond to the necessary t=0 previous pose to timespan nextPose)
        public List<TimeSpan> sequencesTiming;

        public Queue<Pose> queueData;
        public Queue<TimeSpan> queueTiming;

        public Skeleton skeleton;

        public Pose _previousPose;
        public Pose _nextPose;
        public TimeSpan _localTime=new TimeSpan(0);
        public TimeSpan _nextTime;

        public Pose _currentLocalPose;

        public override void Start()
        {
            queueData = new Queue<Pose>(sequencesData);
            queueTiming = new Queue<TimeSpan>(sequencesTiming);
            _previousPose = skeleton.bindPose;
            _nextPose = queueData.Dequeue();
            _nextTime = queueTiming.Dequeue();
            base.Start();
        }

        // delta is in milliseconds
        public override void Update(float _delta)
        {
            if(isPlaying)
            {
                _localTime += new TimeSpan(0, 0, 0, 0, (int)_delta);
                if (_localTime > _nextTime)
                {
                    _nextPose = queueData.Dequeue();
                    _nextTime = queueTiming.Dequeue();
                    _localTime = new TimeSpan(0);
                }
                float Nweight = (float)(_localTime.TotalMilliseconds / _nextTime.TotalMilliseconds);
                float Pweight = 1 - Nweight;

                _currentLocalPose = _previousPose;
                List<Joint> _previousJ = _previousPose.GetAllJoints();
                List<Joint> _nextJ = _nextPose.GetAllJoints();
                for (int i = 0; i < _previousJ.Count; i++)
                {
                    DualQuaternion DQ = DualQuaternion.DLB(new List<DualQuaternion>() { _previousJ[i].localRotationTranslation, _nextJ[i].localRotationTranslation }, new List<float>() { Pweight, Nweight });
                    _currentLocalPose.GetJointByName(_previousJ[i].name).localRotationTranslation = DQ;
                }
            }
        }
    }
}
