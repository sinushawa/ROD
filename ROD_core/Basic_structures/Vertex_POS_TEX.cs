using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Basic_structures
{
    public struct Vertex_POS_TEX
    {
        public Vector3 pos;
        public Vector2 tex;

        public Vertex_POS_TEX(Vector3 _pos, Vector2 _tex)
        {
            pos = _pos;
            tex = _tex;
        }
        public Vertex_POS_TEX(float _x, float _y, float _z, float _u, float _v)
        {
            pos = new Vector3(_x, _y, _z);
            tex = new Vector2(_u, _v);
        }
    }
}
