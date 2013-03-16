using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core
{
    public struct psBuffer
    {
        public Vector4 LightColor;
    }
    public struct vsBuffer
    {
        public Matrix World;
        public Matrix ViewProjection;
        public Vector3 eyePos;
        public float padding3;
        public Vector3 LightPos;
        public float padding1;
    }
}
