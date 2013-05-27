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

        public AnimationSkinningState animation;

        public Skeleton(string _name)
        {
            name = _name;
            animation = new AnimationSkinningState();
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
        }
        public List<DualQuaternion> GetJointWTMList()
        {
            return currentPose.GetWorldTransformVersion().GetJoints(TreeNavigation.Breadth_first).Select(x=> x.localRotationTranslation).ToList();
        }
    }
}
