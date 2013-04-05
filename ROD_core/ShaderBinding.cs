using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using ROD_core.Graphics.Assets;

namespace ROD_core
{
    public class ShaderTypeAttribute : Attribute
    {
        public ShaderTypeAttribute(Type _type)
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
    public struct ByteCodeBind
    {
        public Shaders shaderType;
        public ShaderBytecode byteCode;
        public List<ConstantPack> constants;

        public ByteCodeBind(Shaders _shaderType, ShaderBytecode _byteCode) : this(_shaderType, _byteCode, new List<ConstantPack>())
        {
        }
        public ByteCodeBind(Shaders _shaderType, ShaderBytecode _byteCode, ConstantPack _constant)
        {
            shaderType = _shaderType;
            byteCode = _byteCode;
            constants = new List<ConstantPack>();
            constants.Add(_constant);
        }
        public ByteCodeBind(Shaders _shaderType, ShaderBytecode _byteCode, List<ConstantPack> _constants)
        {
            shaderType = _shaderType;
            byteCode = _byteCode;
            constants = _constants;
        }
    }
    public struct BufferBound
    {
        public SharpDX.Direct3D11.Buffer buffer;
        public Object value;

        public BufferBound(SharpDX.Direct3D11.Buffer _buffer, Object _value)
        {
            buffer = _buffer;
            value = _value;
        }
    }

    public class ShaderSolution
    {
        public string name;
        public Dictionary<Shaders, ShaderBytecode> shaders_bytecode;
        public Dictionary<Shaders, SharpDX.Direct3D11.Buffer[]> shaders_buffers;
        public Dictionary<Shaders, List<ConstantPack>> shaders_constants;
        public VertexShader vs;
        public HullShader hs;
        public DomainShader ds;
        public GeometryShader gs;
        public PixelShader ps;
        public bool HasTesselation = false;
        public bool IsDeferred = false;

        public ShaderSolution(string _name, Device Device, ByteCodeBind[] _shaders_bytecode)
        {
            shaders_bytecode = new Dictionary<Shaders, ShaderBytecode>();
            shaders_buffers = new Dictionary<Shaders, SharpDX.Direct3D11.Buffer[]>();
            shaders_constants = new Dictionary<Shaders, List<ConstantPack>>();
            //shaders_bound_values = new Dictionary<Shaders, NonNullable<SharpDX.DataStream>[]>();
            name = _name;
            for (int i = 0; i < _shaders_bytecode.Length; i++)
            {
                shaders_bytecode.Add(_shaders_bytecode[i].shaderType, _shaders_bytecode[i].byteCode);
                shaders_constants.Add(_shaders_bytecode[i].shaderType, _shaders_bytecode[i].constants);
                switch (_shaders_bytecode[i].shaderType)
                {
                    case Shaders.VertexShader:
                        vs = new VertexShader(Device, _shaders_bytecode[i].byteCode);
                        break;
                    case Shaders.HullShader:
                        hs = new HullShader(Device, _shaders_bytecode[i].byteCode);
                        break;
                    case Shaders.DomainShader:
                        ds = new DomainShader(Device, _shaders_bytecode[i].byteCode);
                        break;
                    case Shaders.GeometryShader:
                        gs = new GeometryShader(Device, _shaders_bytecode[i].byteCode);
                        break;
                    case Shaders.PixelShader:
                        ps = new PixelShader(Device, _shaders_bytecode[i].byteCode);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    [Flags]
    public enum Shaders : long
    {
        [ShaderTypeAttribute(typeof(VertexShader))]
        VertexShader = (1 << 0),
        [ShaderTypeAttribute(typeof(HullShader))]
        HullShader = (1 << 1),
        [ShaderTypeAttribute(typeof(DomainShader))]
        DomainShader = (1 << 2),
        [ShaderTypeAttribute(typeof(GeometryShader))]
        GeometryShader = (1 << 3),
        [ShaderTypeAttribute(typeof(PixelShader))]
        PixelShader = (1 << 4)
    }

    [Flags]
    public enum Technique : long
    {
        None = (1 << 0),
        Diffuse_mapping = (1 << 1),
        Specular_mapping = (1 << 2),
        Normal_mapping = (1 << 3),
        Bump_mapping = (1 << 4),
        Displacement_mapping = (1 << 5),
        Skinning = (1 << 6),
        Hard_shadow = (1 << 7),
        Soft_shadow = (1 << 8),
        Ambient_occlusion = (1 << 9),
        Bloom = (1 << 10),
        Lens_blur = (1 << 11),
        Tesslation = (1 << 12),
        Quad_rendering = (1 << 13)
    }

    static public class ShaderBinding
    {
        static public Dictionary<Technique, ShaderSolution> ShaderPool = new Dictionary<Technique, ShaderSolution>();

        static public ShaderSolution GetCompatibleShader(Model _model)
        {
            ShaderSolution _solution = null;
            Technique necessaryTechnique = Technique.None;
            List<MapSlot> usedMaps = (from q in _model.material.maps select q.Key).ToList<MapSlot>();
            foreach (MapSlot _mapslot in usedMaps)
            {
                long converted = Convert.ToInt64(_mapslot);
                Technique temp = (Technique)converted;
                necessaryTechnique = necessaryTechnique | temp;
            }
            if (_model.isSkinned)
            {
                necessaryTechnique = necessaryTechnique | Technique.Skinning;
            }
            if (_model.isTesselated)
            {
                necessaryTechnique = necessaryTechnique | Technique.Tesslation;
            }
            necessaryTechnique &= ~Technique.None;
            if (ShaderPool.Keys.Contains<Technique>(necessaryTechnique))
            {
                _solution = ShaderPool[necessaryTechnique];
            }
            return _solution;
        }
    }
}
