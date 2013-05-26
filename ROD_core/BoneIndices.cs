using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    /// <summary>Structure to store bone indices in DynamicVertex classes. 
    /// <para>Stores up to 4 indices in index0 ,1 ,2 and 3 of type byte, for unused indices unsed a weight of 0.0</para>
    /// </summary> 
    public struct BoneIndices
    {
        public byte index0;
        public byte index1;
        public byte index2;
        public byte index3;

        public BoneIndices(byte _index0, byte _index1, byte _index2, byte _index3)
        {
            index0 = _index0;
            index1 = _index1;
            index2 = _index2;
            index3 = _index3;
        }
    }
}
