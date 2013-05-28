using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Mathematics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ROD_core.Graphics.Animation
{
    [Serializable]
    public class Clip_Skinning : Clip, ISerializable
    {
        public List<Pose> sequencesData;
        // End time of the coresponding sequences every timing is global (the time correspond to the necessary t=0 origin pose to timespan nextPose)
        public List<TimeSpan> sequencesTiming;

        public Pose previousPose;
        public Pose nextPose;
        public float nweight = 0.0f;

        public TimeSpan _nextTime;
        public TimeSpan _previousTime;

        public Clip_Skinning()
        {
        }
        #region Serialize
        protected Clip_Skinning(SerializationInfo info, StreamingContext context)
        {
            MemoryStream ms = new MemoryStream((byte[])info.GetValue("sequencesData", typeof(byte[])));
            BinaryFormatter bf = new BinaryFormatter();
            sequencesData = (List<Pose>)bf.Deserialize(ms);
            sequencesTiming = (List<TimeSpan>)info.GetValue("sequencesTiming", typeof(List<TimeSpan>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(ms, sequencesData);
            ms.Close();
            info.AddValue("sequencesData", ms.GetBuffer(), typeof(byte[]));
            info.AddValue("sequencesTiming", sequencesTiming, typeof(List<TimeSpan>));
        }
        #endregion

        #region Load and Save
        public static Clip_Skinning createFromFile(string _filename)
        {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream readStream = new FileStream(_filename, FileMode.Open);
            Clip_Skinning loadedClip = (Clip_Skinning)bf.Deserialize(readStream);
            readStream.Close();
            return loadedClip;
        }
        public void saveToFile(string _filename)
        {
            Stream stream = File.Open(_filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
        }
        public static void saveToFile(Clip_Skinning _clip, string _filename)
        {
            _clip.saveToFile(_filename);
        }
        #endregion

        public void Init()
        {
            localTime = new TimeSpan(0);
            previousPose = sequencesData[0];
            nextPose = sequencesData[1];
            _nextTime = sequencesTiming[1];
            _previousTime = new TimeSpan(0);
        }
        public override void Play()
        {
            base.Play();
        }
        public override void Pause()
        {
            base.Pause();
        }
        public override void GoTo(float _delta)
        {
            TimeSpan _timeToReach=new TimeSpan(0, 0, 0, 0, (int)_delta);
            Update((float)(_timeToReach - localTime).TotalMilliseconds);
        }

        // delta is in milliseconds
        public override void Update(float _delta)
        {
            if(isPlaying)
            {
                localTime += new TimeSpan(0, 0, 0, 0, (int)_delta);
            }
            if (localTime > _nextTime)
            {
                if (nextPose == sequencesData[sequencesData.Count - 1])
                {
                    Init();
                }
                else
                {
                    int index = sequencesTiming.IndexOf(sequencesTiming.Last(x => x < localTime));
                    previousPose = sequencesData[index - 1];
                    nextPose = sequencesData[index];
                    _previousTime = sequencesTiming[index - 1];
                    _nextTime = sequencesTiming[index];
                }
            }
            nweight = (float)((localTime.TotalMilliseconds-_previousTime.TotalMilliseconds) / (_nextTime.TotalMilliseconds-_previousTime.TotalMilliseconds));
        }
    }
}
