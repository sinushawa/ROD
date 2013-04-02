using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Mathematics
{
    public struct DualQuaternion
    {
        public Quaternion real;
        public Quaternion dual;

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
        public static DualQuaternion operator *(DualQuaternion q, float scale)
        {
            DualQuaternion resultat = q;
            resultat.real *= scale;
            resultat.dual *= scale;
            return resultat;
        }
        public static DualQuaternion operator +(DualQuaternion _left, DualQuaternion _right)
        {
            return new DualQuaternion(_left.real + _right.real, _left.dual + _right.dual);
        }
        // Multiplication order - left to right
        public static DualQuaternion operator *(DualQuaternion _left, DualQuaternion _right)
        {
            return new DualQuaternion(_right.real * _left.real, _right.dual * _left.real + _right.real * _left.dual);
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
            Quaternion t = (q.dual * 2.0f) * Quaternion.Conjugate(q.real);
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
    }
}
