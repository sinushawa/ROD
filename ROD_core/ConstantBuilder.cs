using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ROD_core
{
    public abstract class ConstantVariable
    {
        public abstract byte[] GetByte();
    }

    public class Constant_Variable<DataType> : ConstantVariable where DataType : struct
    {
        public DataType mDataType;

        public Constant_Variable(ref DataType value)
        {
            mDataType = value;
        }
        public override byte[] GetByte()
        {
            return ConstantBuilder.StructureToBytes(mDataType);
        }
    }
    public class ConstantPack
    {
        private List<ConstantVariable> pack;

        public ConstantPack() : this(new List<ConstantVariable>())
        {
        }
        public ConstantPack(List<ConstantVariable> _pack)
        {
            pack = _pack;
        }
        public void Add<TStruct>(TStruct value) where TStruct : struct
        {
            Constant_Variable<TStruct> _constant = new Constant_Variable<TStruct>(ref value);
            pack.Add(_constant);
        }
        public void Update<TStruct>(TStruct value, int index) where TStruct : struct
        {
            pack.RemoveAt(index);
            Constant_Variable<TStruct> _constant = new Constant_Variable<TStruct>(ref value);
            pack.Insert(index, _constant);
        }
        public void Add(ConstantVariable _constant)
        {
            pack.Add(_constant);
        }

        // 16-byte boundary packing
        public byte[] GetPackedBytes()
        {
            List<byte> listbyte = new List<byte>();
            int count = 0;
            for (int i=0; i<pack.Count; i++)
            {
                byte[] temp = pack[i].GetByte();
                listbyte.AddRange(temp);
                count += temp.Length;
                if( (count % 16) != 0 )
                {
                    int restant = (int)Math.Ceiling(count / (double)16)*16 - count;
                    if (i + 1 < pack.Count)
                    {
                        byte[] next = pack[i + 1].GetByte();
                        if (next.Length > restant)
                        {
                            while (restant > 0)
                            {
                                listbyte.Add((byte)0);
                                count++;
                                restant = (int)Math.Ceiling(count / (double)16)*16 - count;
                            }
                        }
                    }
                    else
                    {
                        while (restant > 0)
                        {
                            listbyte.Add((byte)0);
                            count++;
                            restant = (int)Math.Ceiling(count / (double)16)*16 - count;
                        }
                    }
                }

            }
            return listbyte.ToArray();
        }
    }

    public static class ConstantBuilder
    {
        public static unsafe byte[] StructureToBytes<TStruct>(TStruct st) where TStruct : struct
        {
            var bytes = new byte[Marshal.SizeOf(st)];
            fixed (byte* ptr = bytes) Marshal.StructureToPtr(st, new IntPtr(ptr), true);
            return bytes;
        }
    }
}
