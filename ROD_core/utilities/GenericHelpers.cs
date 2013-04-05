using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ROD_core.utilities
{
    public static class GenericHelpers
    {
        public static int SizeOfG(this Type type)
        {
            // Get the generic type definition
            MethodInfo method = typeof(SharpDX.Utilities).GetMethods(BindingFlags.Static | BindingFlags.Public).Where<MethodInfo>(m => m.IsGenericMethod && m.Name == "SizeOf").FirstOrDefault();
            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(type);
            int valeure = (int)method.Invoke(null, new object[] { });
            return valeure;
        }
        public static int SizeOf(this Type type)
        {
            if (type.IsValueType)
            {
                return Marshal.SizeOf(type);
            }
            else
            {
                return type.SizeOfG();
            }
        }
        public static byte[] ToByte(this object[] args)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, args);

                return ms.ToArray();
            }
        }
    }
}
