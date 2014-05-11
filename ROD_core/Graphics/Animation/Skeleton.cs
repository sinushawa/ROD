using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ROD_core.Mathematics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ROD_core.Graphics.Animation
{
    [Flags]
    public enum SkeletonArchetype
    {
        None = (1 << 0),
        Biped = (1 << 1),
        Quadruped = (1 << 1)
    }
    [Serializable]
    public class Skeleton : ISerializable
    {
        public string name;
        public Guid id;
        public SkeletonArchetype skeletonType;
        public int jointCount;
        public Pose bindPose;
        public Pose currentPose;
        public DualQuaternion[] BonePalette;

        private AnimationSkinningState animation;
        public AnimationSkinningState Animation
        {
            get
            {
                return animation;
            }
            set
            {
                animation = (AnimationSkinningState)value;
            }
        }

        public Skeleton(string _name)
        {
            name = _name;
            animation = new AnimationSkinningState();
        }
        public Skeleton(string _name, Pose _bindPose)
        {
            name = _name;
            bindPose = _bindPose;
            animation = new AnimationSkinningState();
            jointCount = bindPose.joints.Count;
            BonePalette = new DualQuaternion[jointCount];
        }

        #region Serialize
        protected Skeleton(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            jointCount = (int)info.GetValue("jointCount", typeof(int));
            bindPose = (Pose)info.GetValue("bindPose", typeof(Pose));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name, typeof(string));
            info.AddValue("jointCount", jointCount, typeof(int));
            info.AddValue("bindPose", bindPose, typeof(Pose));
        }
        #endregion

        #region Load and Save
        public static Skeleton createFromFile(string _filename)
        {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream readStream = new FileStream(_filename, FileMode.Open);
            Skeleton loadedSkeleton = (Skeleton)bf.Deserialize(readStream);
            readStream.Close();
            loadedSkeleton.BonePalette = new DualQuaternion[loadedSkeleton.jointCount];
            loadedSkeleton.animation = new AnimationSkinningState();
            return loadedSkeleton;
        }
        public void saveToFile(string _filename)
        {
            Stream stream = File.Open(_filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, this);
            stream.Close();
        }
        public static void saveToFile(Skeleton _skeleton, string _filename)
        {
            _skeleton.saveToFile(_filename);
        }
        #endregion

        public void Update(float _delta)
        {
            currentPose = animation.ComputeLocalPose(_delta);
            //currentPose.CalculateWorldTransform(bindPose);
            //currentPose = animation.clips[0].nextPose;
            GetJointWTMList();
        }
        public List<DualQuaternion> GetJointWTMList()
        {
            //currentPose.ComputeWorldRotationTranslation();
            List<DualQuaternion> CDQts = currentPose.ComputeWorldRotationTranslation();
            BonePalette = CDQts.ToArray();
            return CDQts;
        }
        public void Precalculate()
        {
            foreach (Clip_Skinning _clip in animation.clips)
            {
                foreach (Pose _pose in _clip.sequencesData)
                {
                    foreach (Joint _joint in _pose.joints)
                    {
                        DualQuaternion WorldBinding = bindPose.GetJointById(_joint.id).worldRotationTranslation;
                        _joint.worldRotationTranslation = WorldBinding.Conjugate() * _joint.localRotationTranslation * WorldBinding;
                    }
                }
            }
        }
    }
}
