using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ROD_core.Graphics.Animation;

namespace ROD_core.Graphics.Assets
{
    public class Entity : Model
    {
        public Skeleton skeleton;

        public Entity(Mesh _mesh, Material _material) : this(_mesh, _material, false, false)
        {
        }
        public Entity(Mesh _mesh, Material _material, bool _isTesselated, bool _isSkinned): base(_mesh, _material, _isTesselated, _isSkinned)
        {
        }
    }
}
