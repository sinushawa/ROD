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
        public UInt32 index0;
        public UInt32 index1;
        public UInt32 index2;
        public UInt32 index3;

        public BoneIndices(int _index0, int _index1, int _index2, int _index3)
        {
            index0 = (UInt32)_index0;
            index1 = (UInt32)_index1;
            index2 = (UInt32)_index2;
            index3 = (UInt32)_index3;
        }
    }
}
