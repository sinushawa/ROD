using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;


namespace ROD_core
{
	public class SemanticAttribute : Attribute
	{
		public SemanticAttribute(Format _inputFormat, Type _type)
		{
			this.inputFormat = _inputFormat;
			this.inputType = _type;
		}
		private Type inputType;
		public Type InputType
		{
			get { return inputType; }
			set { inputType = value; }
		}
		private Format inputFormat;
		public Format InputFormat
		{
			get { return inputFormat; }
			set { inputFormat = value; }
		}
	}
    
	[Flags]
	public enum Semantic : long
	{
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32_Float, typeof(Vector3))]
		POSITION = (1 << 0),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32_Float, typeof(Vector3))]
		NORMAL = (1 << 1),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32_Float, typeof(Vector2))]
		TEXCOORD = (1 << 2),
        [SemanticAttribute(SharpDX.DXGI.Format.R32G32_Float, typeof(Vector2))]
        TEXCOORD2 = (1 << 3),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32A32_Float, typeof(Vector4))]
		COLOR = (1 << 4),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32_Float, typeof(Vector3))]
		BINORMAL = (1 << 5),
        [SemanticAttribute(SharpDX.DXGI.Format.R32G32B32A32_UInt, typeof(BoneIndices))]
		BONEINDEX = (1 << 6),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32A32_Float, typeof(Vector4))]
		BONEWEIGHT = (1 << 7),
		[SemanticAttribute(SharpDX.DXGI.Format.R32_Float, typeof(float))]
		PSIZE = (1 << 8),
		[SemanticAttribute(SharpDX.DXGI.Format.R32G32B32_Float, typeof(Vector3))]
		TANGENT = (1 << 9)
	}

	public class InputElementAttribute : Attribute
	{
		public InputElementAttribute(string _semantic, Format _inputFormat)
		{
			this.semantic = _semantic;
			this.inputFormat = _inputFormat;
		}
		private string semantic;
		public string Semantic
		{
			get { return semantic; }
			set { semantic = value; }
		}
		private Format inputFormat;
		public Format InputFormat
		{
			get { return inputFormat; }
			set { inputFormat = value; }
		}
	}
	

	public static class DynamicVertex
	{

		public static Type CreateVertex(Semantic vertexDefinition)
		{
			AppDomain appDomain = AppDomain.CurrentDomain;
			AssemblyName _assemblyName = new AssemblyName("DynamicVertex");
			AssemblyBuilder _assemblyBuilder = appDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder _moduleBuilder = _assemblyBuilder.DefineDynamicModule("DynamicModule");
			string _structName = "Vertex";
			List<Semantic> elems = vertexDefinition.getVertexDefinition();
			foreach(Semantic _semantic in elems)
			{
				_structName += _semantic.ToString();
			}
			TypeBuilder _typeBuilder = _moduleBuilder.DefineType(_structName, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable, typeof(ValueType));
			List<Type> _types = new List<Type>();
			List<FieldBuilder> _fb = new List<FieldBuilder>();
			foreach (Semantic _semantic in elems)
			{
				string _name = _semantic.ToString();
				SharpDX.DXGI.Format _format = _semantic.GetFormat();
				Type _type = _semantic.GetFormatType();
				FieldBuilder _fieldBuilder = _typeBuilder.DefineField(_name.ToLower(), _type, FieldAttributes.Public);
				_types.Add(_type);
				Type[] ctorParams = new Type[] { typeof(string), typeof(SharpDX.DXGI.Format) };
				ConstructorInfo classCtorInfo = typeof(InputElementAttribute).GetConstructor(ctorParams);
				CustomAttributeBuilder myCABuilder = new CustomAttributeBuilder(classCtorInfo, new object[] { _name, _format});
				_fieldBuilder.SetCustomAttribute(myCABuilder);
				_fb.Add(_fieldBuilder);
				
			}
			ConstructorBuilder _constructorBuilder = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, _types.ToArray<Type>());
			ILGenerator ilGenerator = _constructorBuilder.GetILGenerator();
			for (byte i=0; i<_fb.Count; i++)
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Ldarg_S, i+1);
				ilGenerator.Emit(OpCodes.Stfld, _fb[i]);
			}
			ilGenerator.Emit(OpCodes.Ret);                          // return

			return _typeBuilder.CreateType();;
		}
	}

	public static class AttributeDefinition
	{
		public static List<Semantic> getVertexDefinition(this Semantic value)
		{
			List<Semantic> vertexDefinition = new List<Semantic>();
			List<Semantic> semanticList= EnumToList<Semantic>();
			foreach (Semantic _semantic in semanticList)
			{
				if ((value & _semantic) == _semantic)
				{
					vertexDefinition.Add(_semantic);
				}
			}
			return vertexDefinition;
		}
		public static List<T> EnumToList<T>()
		{
			Type enumType = typeof(T);

			// Can't use type constraints on value types, so have to do check like this
			if (enumType.BaseType != typeof(Enum))
				throw new ArgumentException("T must be of type System.Enum");

			Array enumValArray = Enum.GetValues(enumType);

			List<T> enumValList = new List<T>(enumValArray.Length);

			foreach (long val in enumValArray)
			{
				enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
			}

			return enumValList;
		}

		public static SharpDX.DXGI.Format GetFormat(this Enum value)
		{
			Type type = value.GetType();

			// Get fieldinfo for this type
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Get the stringvalue attributes
			SemanticAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(SemanticAttribute), false) as SemanticAttribute[];

			// Return the first if there was a match.
			try
			{
				return attribs[0].InputFormat;
			}
			catch(Exception ex)
			{
				throw new SystemException(ex.Message);
			}
		}
		public static Type GetFormatType(this Semantic value)
		{
			Type type = value.GetType();

			// Get fieldinfo for this type
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Get the stringvalue attributes
			SemanticAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(SemanticAttribute), false) as SemanticAttribute[];

			// Return the first if there was a match.
			try
			{
				return attribs[0].InputType;
			}
			catch (Exception ex)
			{
				throw new SystemException(ex.Message);
			}
		}
        public static Semantic reorderDefinition(this Semantic value)
        {
            List<Semantic> vertexDefinition = value.getVertexDefinition();
            Semantic reordered=vertexDefinition[0];
            for (int i = 1; i < vertexDefinition.Count; i++ )
            {
                reordered |= vertexDefinition[i];
            }

            return reordered;
        }

		public static InputElement[] GetInputElements(this Semantic value)
		{
			List<Semantic> vertexDefinition=value.getVertexDefinition();
			List<InputElement> listInputElements = new List<InputElement>();
			int offset = 0;
			foreach (Semantic elem in vertexDefinition)
			{
				Type type = typeof(Semantic);

				// Get fieldinfo for this type
				FieldInfo fieldInfo = type.GetField(elem.ToString());
				// Get the stringvalue attributes
				SemanticAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(SemanticAttribute), false) as SemanticAttribute[];
				listInputElements.Add(new InputElement(elem.ToString(), 0, attribs[0].InputFormat, offset, 0));
				offset += (int)SharpDX.DXGI.FormatHelper.SizeOfInBytes(attribs[0].InputFormat);
			}
			return listInputElements.ToArray<InputElement>();
		}
		public static InputElement[] GetInputElements(this Type type)
		{
			FieldInfo[] fieldInfo = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
			List<InputElement> listInputElements = new List<InputElement>();
			int offset = 0;
			// Return the first if there was a match.
			foreach (FieldInfo fi in fieldInfo)
			{
				// Get the stringvalue attributes
				InputElementAttribute[] attribs = fi.GetCustomAttributes(typeof(InputElementAttribute), false) as InputElementAttribute[];
				if (attribs.Length > 0)
				{

					listInputElements.Add(new InputElement(attribs[0].Semantic, 0, attribs[0].InputFormat, offset, 0));
					offset += (int)SharpDX.DXGI.FormatHelper.SizeOfInBytes(attribs[0].InputFormat);
				}
			}
			return listInputElements.ToArray<InputElement>();
		}
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
	}
}
