using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ROD_core.utilities
{
    public static class DynamicHelpers
    {
        public static List<Semantic> getVertexDefinition(this Semantic value)
        {
            List<Semantic> vertexDefinition = new List<Semantic>();
            List<Semantic> semanticList = EnumToList<Semantic>();
            foreach (Semantic _semantic in semanticList)
            {
                if ((value & _semantic) == _semantic)
                {
                    vertexDefinition.Add(_semantic);
                }
            }
            return vertexDefinition;
        }
        public static List<Constants> getConstantDefinition(this Constants value)
        {
            List<Constants> constantDefinition = new List<Constants>();
            List<Constants> semanticList = EnumToList<Constants>();
            foreach (Constants _constant in semanticList)
            {
                if ((value & _constant) == _constant)
                {
                    constantDefinition.Add(_constant);
                }
            }
            return constantDefinition;
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
            catch (Exception ex)
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
        public static Type GetConstantType(this Constants value)
        {
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            ConstantAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(ConstantAttribute), false) as ConstantAttribute[];

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
            Semantic reordered = vertexDefinition[0];
            for (int i = 1; i < vertexDefinition.Count; i++)
            {
                reordered |= vertexDefinition[i];
            }

            return reordered;
        }
        public static Constants reorderDefinition(this Constants value)
        {
            List<Constants> constantDefinition = value.getConstantDefinition();
            Constants reordered = constantDefinition[0];
            for (int i = 1; i < constantDefinition.Count; i++)
            {
                reordered |= constantDefinition[i];
            }

            return reordered;
        }

        public static InputElement[] GetInputElements(this Semantic value)
        {
            List<Semantic> vertexDefinition = value.getVertexDefinition();
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
    }
}
