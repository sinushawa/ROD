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
    }
}
