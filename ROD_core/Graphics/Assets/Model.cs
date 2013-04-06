using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace ROD_core.Graphics.Assets
{
    public class Model : Component
    {
        public string modelName;
        public Mesh mesh;
        public bool isSkinned = false;
        public bool isTesselated = false;
        public Material material;
        public Technique associatedTechnique;
        private InputLayout layout;
        private ROD_core.ShaderSolution _shaderSolution;
        private SamplerState sampler;


        public Model()
        {

        }
        public Model(Mesh _mesh)
        {
            mesh = _mesh;
            modelName = _mesh.meshName;
        }
        public Model(Mesh _mesh, Material _material)
        {
            mesh = _mesh;
            material = _material;
            modelName = _mesh.meshName;
        }
        public Model(Mesh _mesh, Material _material, bool _isTesselated)
        {
            mesh = _mesh;
            material = _material;
            isTesselated = _isTesselated;
            modelName = _mesh.meshName;
        }

        public void Initialize(Device Device)
        {
            ToDispose(mesh);
            _shaderSolution=ROD_core.ShaderBinding.GetCompatibleShader(this);
            layout = new InputLayout(Device, _shaderSolution.shaders_bytecode[Shaders.VertexShader], mesh._vertexStream.vertexDefinition.GetInputElements());
            
            mesh.Load(Device);
            material.LoadTextures(Device);
            sampler = new SamplerState(Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = new SharpDX.Color4(0,0,0,1),
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });
        }

        public void Render(DeviceContext context)
        {
            context.InputAssembler.InputLayout = layout;
            if (isTesselated)
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;
            }
            else
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mesh.vertexBuffer, mesh._vertexStream.vertexType.SizeOf(), 0));
            context.InputAssembler.SetIndexBuffer(mesh.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

            context.VertexShader.Set(_shaderSolution.vs);
            for (int i = 0; i < _shaderSolution.shaders_buffers[Shaders.VertexShader].Length; i++)
            {
                context.VertexShader.SetConstantBuffer(i, _shaderSolution.shaders_buffers[Shaders.VertexShader][i]);
                context.UpdateSubresource<byte>(_shaderSolution.shaders_constants[Shaders.VertexShader][i].cache, _shaderSolution.shaders_buffers[Shaders.VertexShader][i]);
            }
            context.PixelShader.Set(_shaderSolution.ps);
            for (int i = 0; i < _shaderSolution.shaders_buffers[Shaders.PixelShader].Length; i++)
            {
                context.PixelShader.SetConstantBuffer(i, _shaderSolution.shaders_buffers[Shaders.PixelShader][i]);
                context.UpdateSubresource<byte>(_shaderSolution.shaders_constants[Shaders.PixelShader][i].cache, _shaderSolution.shaders_buffers[Shaders.PixelShader][i]);
            }
            if (this.isTesselated)
            {
                context.HullShader.Set(_shaderSolution.hs);
                for (int i = 0; i < _shaderSolution.shaders_buffers[Shaders.HullShader].Length; i++)
                {
                    context.HullShader.SetConstantBuffer(i, _shaderSolution.shaders_buffers[Shaders.HullShader][i]);
                    context.UpdateSubresource<byte>(_shaderSolution.shaders_constants[Shaders.HullShader][i].cache, _shaderSolution.shaders_buffers[Shaders.HullShader][i]);
                }
                context.DomainShader.Set(_shaderSolution.ds);
                for (int i = 0; i < _shaderSolution.shaders_buffers[Shaders.DomainShader].Length; i++)
                {
                    context.DomainShader.SetConstantBuffer(i, _shaderSolution.shaders_buffers[Shaders.DomainShader][i]);
                    context.UpdateSubresource<byte>(_shaderSolution.shaders_constants[Shaders.DomainShader][i].cache, _shaderSolution.shaders_buffers[Shaders.DomainShader][i]);
                }
            }
            else
            {
                context.HullShader.Set(null);
                context.DomainShader.Set(null);
            }
            for(int i = 0; i<material.textures.Count; i++)
            {
                context.PixelShader.SetShaderResource(i, material.textures[i]);
            }
            context.PixelShader.SetSampler(0, sampler);
            context.DrawIndexed(mesh._indexStream.getIndexCount(), 0, 0);
        }
    }
}
