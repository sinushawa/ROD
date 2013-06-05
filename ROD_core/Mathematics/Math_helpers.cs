using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Mathematics
{
    public static class Math_helpers
    {
        // we consider Z (depth not height) as the second component of the projection plane
        public static Vector3 ProjectVectorOnPlane(Vector3 Projector, Vector3 planeMember)
        {
            Vector3 n = Vector3.Cross(-Vector3.UnitZ, planeMember);
            Vector3 projectedVector = Vector3.Subtract(Projector, (Vector3.Dot(Projector, n) * n));
            return projectedVector;
        }
        public static float ToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180.0);
        }
        public static float ToDegrees(float radians)
        {
            return (float)(radians * 180.0 / Math.PI);
        }
        public static float GetYaw(Quaternion q)
        {
            float temp = -2 * (q.X * q.Z - q.W * q.Y);
            float temp2 = (float)Math.Truncate(temp);
            float temp3 = temp - temp2;
            float temp4 = (float)Math.Asin(temp3);
            float temp5 = (float)Math.Round(temp2 * (float)Math.PI / 2.0f + temp4, 2);
            return temp5;
        }
        public static float GetPitch(Quaternion q)
        {
            float temp = (float)Math.Atan2(2 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
            float temp3 = (float)Math.Round(temp, 2);
            return temp3;
        }
        public static float GetRoll(Quaternion q)
        {
            float temp = (float)Math.Atan2(2 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);
            float temp3 = (float)Math.Round(temp, 2);
            return temp3;
        }
        public static Vector3 GetRollYawPitch(Quaternion q)
        {
            return new Vector3(ToDegrees(GetRoll(q)), ToDegrees(GetYaw(q)), ToDegrees(GetPitch(q)));
        }
    }
}
