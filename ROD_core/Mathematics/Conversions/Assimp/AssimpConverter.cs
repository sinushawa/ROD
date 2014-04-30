using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using SharpDX;
using Quaternion = Assimp.Quaternion;

namespace ROD_core.Mathematics.Conversions.Assimp
{
    public static class AssimpConverter
    {
        public static SharpDX.Quaternion ConvertTo(this Quaternion _input)
        {
            SharpDX.Quaternion _q = new SharpDX.Quaternion(_input.X, _input.Y, _input.Z, _input.W);
            return _q;
        }
        public static SharpDX.Vector3 ConvertTo(this Vector3D _input)
        {
            Vector3 _v = new Vector3(_input.X, _input.Y, _input.Z);
            return _v;
        }
    }
}
