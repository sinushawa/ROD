using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using ROD_core.utilities;

namespace ROD_core
{
    public class ConstantAttribute : Attribute
    {
        public ConstantAttribute(Type _type)
        {
            this.inputType = _type;
        }
        private Type inputType;
        public Type InputType
        {
            get { return inputType; }
            set { inputType = value; }
        }
    }

    [Flags]
    public enum Constants : long
    {
        [ConstantAttribute(typeof(Matrix))]
        WorldMatrix = (1 << 0),
        [ConstantAttribute(typeof(Matrix))]
        ViewProjectionMatrix = (1 << 1),
        [ConstantAttribute(typeof(Vector3))]
        EyePosition = (1 << 2),
        [ConstantAttribute(typeof(Vector3))]
        LightPosition = (1 << 3)
    }

    public static  class DynamicConstant
    {
        public static Type CreateConstant(Constants constantDefinition)
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            AssemblyName _assemblyName = new AssemblyName("DynamicConstant");
            AssemblyBuilder _assemblyBuilder = appDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder _moduleBuilder = _assemblyBuilder.DefineDynamicModule("DynamicModule");
            string _structName = "Constant";
            List<Constants> elems = constantDefinition.getConstantDefinition();
            foreach (Constants _constant in elems)
            {
                _structName += _constant.ToString();
            }
            TypeBuilder _typeBuilder = _moduleBuilder.DefineType(_structName, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable, typeof(ValueType));
            List<Type> _types = new List<Type>();
            List<FieldBuilder> _fb = new List<FieldBuilder>();
            foreach (Constants _constant in elems)
            {
                string _name = _constant.ToString();
                Type _type = _constant.GetConstantType();
                FieldBuilder _fieldBuilder = _typeBuilder.DefineField(_name.ToLower(), _type, FieldAttributes.Public);
                _types.Add(_type);
                _fb.Add(_fieldBuilder);

            }
            ConstructorBuilder _constructorBuilder = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, _types.ToArray<Type>());
            ILGenerator ilGenerator = _constructorBuilder.GetILGenerator();
            for (byte i = 0; i < _fb.Count; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_S, i + 1);
                ilGenerator.Emit(OpCodes.Stfld, _fb[i]);
            }
            ilGenerator.Emit(OpCodes.Ret);                          // return

            return _typeBuilder.CreateType(); ;
        }
    }
}
