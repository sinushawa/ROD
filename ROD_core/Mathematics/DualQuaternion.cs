using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ROD_core.Mathematics
{
    [Serializable]
    public struct DualQuaternion : ISerializable
    {
        public Quaternion real;
        public Quaternion dual;

        public Vector3 RollPitchYaw
        {
            get
            {
                return Math_helpers.GetRollYawPitch(this.real);
            }
        }
        public Vector3 Axis
        {
            get
            {
                return GetRotation(this).Axis;
            }
        }
        public float Angle
        {
            get
            {
                return GetRotation(this).Angle;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return GetRotation(this);
            }
        }
        public Vector3 Translation
        {
            get
            {
                return GetTranslation(this);
            }
        }

        public DualQuaternion(Quaternion _real, Quaternion _dual)
        {
            real = _real;
            dual = _dual;
        }
        public DualQuaternion(Quaternion _real, Vector3 _t)
        {
            real = _real;
            real.Normalize();
            dual = (new Quaternion(_t, 0) * real) * 0.5f;
        }
        private DualQuaternion(SerializationInfo info, StreamingContext context)
        {
            real = (Quaternion)info.GetValue("real", typeof(Quaternion));
            dual = (Quaternion)info.GetValue("dual", typeof(Quaternion));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("real", real, typeof(Quaternion));
            info.AddValue("dual", dual, typeof(Quaternion));
        }

        /// <summary>
        /// The identity <see cref="ROD_core.Mathematics.DualQuaternion"/> (0, 0, 0, 1) (0, 0, 0, 0).
        /// </summary>
        public static readonly DualQuaternion Identity = new DualQuaternion(new Quaternion (0.0f, 0.0f, 0.0f, 1.0f), new Quaternion(0,0,0,0));

        public static float Dot(DualQuaternion a, DualQuaternion b)
        {
            return Quaternion.Dot(a.real, b.real);
        }
        public static DualQuaternion Normalize(DualQuaternion q)
        {
            float mag = Quaternion.Dot(q.real, q.real);
            DualQuaternion resultat = q;
            resultat.real *= 1.0f / mag;
            resultat.dual *= 1.0f / mag;
            return resultat;
        }
        public void Normalize()
        {
            this=DualQuaternion.Normalize(this);
        }
        public static DualQuaternion operator *(DualQuaternion q, float scale)
        {
            DualQuaternion resultat = q;
            resultat.real *= scale;
            resultat.dual *= scale;
            return resultat;
        }
        public static DualQuaternion operator /(DualQuaternion q, float scale)
        {
            DualQuaternion resultat = q;
            resultat.real *= 1.0f/scale;
            resultat.dual *= 1.0f/scale;
            return resultat;
        }
        public static DualQuaternion operator +(DualQuaternion _left, DualQuaternion _right)
        {
            return new DualQuaternion(_left.real + _right.real, _left.dual + _right.dual);
        }
        // Multiplication order - left to right
        public static DualQuaternion operator *(DualQuaternion _left, DualQuaternion _right)
        {
            return new DualQuaternion( _right.real*_left.real, _right.real*_left.dual +_right.dual*_left.real);
        }
        public static DualQuaternion Conjugate(DualQuaternion q)
        {
            return new DualQuaternion(Quaternion.Conjugate(q.real), Quaternion.Conjugate(q.dual));
        }
        public static Quaternion GetRotation(DualQuaternion q)
        {
            return q.real;
        }
        public static Vector3 GetTranslation(DualQuaternion q)
        {
            Quaternion t =Quaternion.Conjugate(q.real)* (q.dual * 2.0f) ;
            return new Vector3(t.X, t.Y, t.Z);
        }
        public static Matrix DualQuaternionToMatrix(DualQuaternion q)
        {
            q = DualQuaternion.Normalize(q);
            Matrix M = Matrix.Identity;
            float w = q.real.W;
            float x = q.real.X;
            float y = q.real.Y;
            float z = q.real.Z;
            // Extract rotational information
            M.M11 = w * w + x * x - y * y - z * z;
            M.M12 = 2 * x * y + 2 * w * z;
            M.M13 = 2 * x * z - 2 * w * y;
            M.M21 = 2 * x * y - 2 * w * z;
            M.M22 = w * w + y * y - x * x - z * z;
            M.M23 = 2 * y * z + 2 * w * x;
            M.M31 = 2 * x * z + 2 * w * y;
            M.M32 = 2 * y * z - 2 * w * x;
            M.M33 = w * w + z * z - x * x - y * y;
            // Extract translation information
            Quaternion t = (q.dual * 2.0f) * Quaternion.Conjugate(q.real);
            M.M41 = t.X;
            M.M42 = t.Y;
            M.M43 = t.Z;
            return M;
        }

        public float Length()
        {
            return real.Length();
        }

        public static DualQuaternion DLB(List<DualQuaternion> quaternions, List<float> weights)
        {
            DualQuaternion blendDQ = quaternions[0]*weights[0];
            for (int i = 1; i < quaternions.Count; i++)
            {
                blendDQ += quaternions[i] * weights[i];
            }
            blendDQ.Normalize();
            return blendDQ;
        }
    }
    public static class DualQuaternionExtension
    {
        public static Vector3 TransformByDQ(this Vector3 _point, DualQuaternion DQ)
        {
            Vector3 realDQXYZ = new Vector3(DQ.real.X, DQ.real.Y, DQ.real.Z);
            Vector3 dualDQXYZ = new Vector3(DQ.dual.X, DQ.dual.Y, DQ.dual.Z);
            return _point + 2 * Vector3.Cross(realDQXYZ, Vector3.Cross(realDQXYZ, _point) + DQ.real.W * _point) + 2 * (DQ.real.W * dualDQXYZ - DQ.dual.W * realDQXYZ + Vector3.Cross(realDQXYZ, dualDQXYZ));
        }
    }
}
