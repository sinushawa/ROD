using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SharpDX;

namespace ROD_core.Graphics.Animation
{
    [Serializable]
    public struct CurveHandle : ISerializable
    {
        public Vector2 easeIn;
        public Vector2 easeOut;

        public CurveHandle(Vector2 _easeIn, Vector2 _easeOut)
        {
            easeIn = _easeIn;
            easeOut = _easeOut;
        }

        private CurveHandle(SerializationInfo info, StreamingContext context)
        {
            easeIn = (Vector2)info.GetValue("easeIn", typeof(Vector2));
            easeOut = (Vector2)info.GetValue("easeOut", typeof(Vector2));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("easeIn", easeIn, typeof(Vector2));
            info.AddValue("easeOut", easeOut, typeof(Vector2));
        }
    }
}
