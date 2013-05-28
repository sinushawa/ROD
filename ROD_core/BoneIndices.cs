using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    /// <summary>Structure to store bone indices in DynamicVertex classes. 
    /// <para>Stores up to 4 indices in index0 ,1 ,2 and 3 of type uint, for unused indices use a weight of 0.0</para>
    /// </summary> 
    public struct BoneIndices
    {
        public uint index0;
        public uint index1;
        public uint index2;
        public uint index3;

        public BoneIndices(int _index0, int _index1, int _index2, int _index3)
        {
            index0 = (uint)_index0;
            index1 = (uint)_index1;
            index2 = (uint)_index2;
            index3 = (uint)_index3;
        }
    }
}
