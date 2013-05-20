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

        public Entity()
        {
            isSkinned = true;
        }
    }
}
