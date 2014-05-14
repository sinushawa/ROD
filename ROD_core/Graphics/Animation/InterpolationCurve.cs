using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core.Graphics.Animation
{
    public class InterpolationCurve
    {
        public CurveHandle beginning;
        public CurveHandle end;

        public InterpolationCurve(CurveHandle _beginning, CurveHandle _end)
        {
            beginning = _beginning;
            end = _end;
        }

        public float Interpolate(float _timing)
        {
            Vector2 resultingPoint = (float)Math.Pow((1 - _timing), 3) * Vector2.Zero + 3 * (float)Math.Pow((1 - _timing), 2) * _timing * beginning.easeOut + 3 * (float)Math.Pow((1 - _timing), 2) * _timing * end.easeIn + (float)Math.Pow((1 - _timing), 3) * new Vector2(1, 1);
            return resultingPoint.Y;
        }
    }
}
