using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    [Flags]
    public enum Constants : long
    {
        None = (1 << 0),
        WorldMatrix = (1 << 1),
        ViewProjectionMatrix = (1 << 2),
        EyePosition = (1 << 3),
        LightPosition = (1 << 4),
        Displacement_mapping = (1 << 5),
        Skinning = (1 << 6),
        Hard_shadow = (1 << 7),
        Soft_shadow = (1 << 8),
        Ambient_occlusion = (1 << 9),
        Bloom = (1 << 10),
        Lens_blur = (1 << 11),
        Tesslation = (1 << 12),
        Quad_rendering = (1 << 13)
    }

    public class DynamicConstantStruct
    {
        
    }
}
