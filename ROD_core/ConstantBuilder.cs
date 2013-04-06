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
        public abstract void Update(ref object _data);
    }

    public class Constant_Variable<DataType> : ConstantVariable where DataType : struct
    {
        public DataType mDataType;
        private Type dataType;

        public Constant_Variable(ref DataType value)
        {
            mDataType = value;
        }
        public override void Update(ref object _data)
        {
            if (!(_data is DataType)) 
            {
                throw new NotImplementedException();
            }
            mDataType = (DataType)_data;
        }
        public override byte[] GetByte()
        {
            return ConstantBuilder.StructureToBytes(mDataType);
        }
    }
    public class ConstantPack
    {
        private List<KeyValuePair<string, ConstantVariable>> pack;
        public byte[] cache;

        public ConstantPack()
        {
            ShaderBinding.VariablesPool.Add(this, new List<string>());
            pack = new List<KeyValuePair<string, ConstantVariable>>();
        }
        public void Add<TStruct>(string name, TStruct value) where TStruct : struct
        {
            ShaderBinding.VariablesPool[this].Add(name);
            Constant_Variable<TStruct> _constant = new Constant_Variable<TStruct>(ref value);
            pack.Add(new KeyValuePair<string, ConstantVariable>(name, _constant));
        }
        public void Add(KeyValuePair<string, ConstantVariable> _keyPair)
        {
            ShaderBinding.VariablesPool[this].Add(_keyPair.Key);
            Add(_keyPair.Key, _keyPair.Value);
        }
        public void Add(string name, ConstantVariable _constant)
        {
            ShaderBinding.VariablesPool[this].Add(name);
            pack.Add(new KeyValuePair<string, ConstantVariable>(name, _constant));
        }
        public void Update<TStruct>(string name, TStruct value) where TStruct : struct
        {
            int id = pack.Select((item, index) => new { itemname = item.Key, itemIndex = index }).Where(x => x.itemname == name).First().itemIndex;
            pack.RemoveAt(id);
            Constant_Variable<TStruct> _constant = new Constant_Variable<TStruct>(ref value);
            pack.Insert(id, new KeyValuePair<string, ConstantVariable>(name, _constant));
        }

        // 16-byte boundary packing
        public void CacheBytes()
        {
            List<byte> listbyte = new List<byte>();
            int count = 0;
            for (int i=0; i<pack.Count; i++)
            {
                byte[] temp = pack[i].Value.GetByte();
                listbyte.AddRange(temp);
                count += temp.Length;
                if( (count % 16) != 0 )
                {
                    int restant = (int)Math.Ceiling(count / (double)16)*16 - count;
                    if (i + 1 < pack.Count)
                    {
                        byte[] next = pack[i + 1].Value.GetByte();
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
            cache = listbyte.ToArray();
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
