using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using System.Reflection;

namespace ROD_engine_DX11
{

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
    
    public struct VertexPNT
    {

        [InputElementAttribute("POSITION", Format.R32G32B32_Float)]
        public Vector3 position;
        [InputElementAttribute("NORMAL", Format.R32G32B32_Float)]
        public Vector3 normal;
        [InputElementAttribute("TEXCOORD", Format.R16G16_Float)]
        public Vector2 texcoord;

        public static string name = "VertexPositionNormalTextcoord";

        public VertexPNT(Vector3 _position, Vector3 _normal, Vector2 _textcoord)
        {
            this.position = _position;
            this.normal = _normal;
            this.texcoord = _textcoord;
            
        }
    }
    struct VertexPNC
    {
        [InputElementAttribute("POSITION", Format.R32G32B32_Float)]
        public Vector3 position;  // declaration order must be constantly identical
        [InputElementAttribute("NORMAL", Format.R32G32B32_Float)]
        public Vector3 normal;
        [InputElementAttribute("COLOR", Format.R32G32B32A32_Float)]
        public Vector4 color;

        public VertexPNC(float x, float y, float z, float nx, float ny, float nz, float r, float g, float b, float a)
        {
            position = new Vector3(x, y, z);
            normal = new Vector3(nx, ny, nz);
            color = new Vector4(r, g, b, a);
        }
    }

    public static class VertexConstructor
    {
        public static InputElement[] GetInputElements(this object value)
        {
            Type type = value.GetType();
            FieldInfo[] fieldInfo = type.GetFields(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public);
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
                    offset+=(int)SharpDX.DXGI.FormatHelper.SizeOfInBytes(attribs[0].InputFormat);
                }
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
