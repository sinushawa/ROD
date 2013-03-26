using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using ROD_core.Graphics.Assets;

namespace ROD_core.RenderToTexture
{
	public class ScreenQuad : Model
	{
		#region Variables / Properties
		//public int screenWidth { get; private set; }
		//public int screenHeight { get; private set; }
		public float quadWidth { get; private set; }
		public float quadHeight { get; private set; }

		public float positionX { get; private set; }
		public float positionY { get; private set; }

		private InputLayout layout;
		private ROD_core.ShaderSolution _shaderSolution;
		private SamplerState sampler;

		#endregion

		#region Constructors
		public ScreenQuad()
		{
            ToDispose(mesh);
		}
		#endregion

		#region Methods
		public bool CreateMesh(Device device, float _quadWidth, float _quadHeight)
		{
			// Store the screen size.
			quadWidth = _quadWidth;
			quadHeight = _quadHeight;

			// Initialize the previous rendering position to negative one.
			positionX = 0.0f;
			positionY = 0.0f;

			mesh = new Mesh();
			Semantic vertexDefinition = Semantic.POSITION | Semantic.TEXCOORD;
			Type vertexType = DynamicVertex.CreateVertex(vertexDefinition);
			
			// better to use (-1,-1) (1,1) to be size independent
			/*
			// Calculate the screen coordinates of the left side of the bitmap.
			var left = (-(screenWidth >> 1)) + (float)positionX;
			// Calculate the screen coordinates of the right side of the bitmap.
			var right = left + quadWidth;
			// Calculate the screen coordinates of the top of the bitmap.
			var top = (screenHeight >> 1) - (float)positionY;
			// Calculate the screen coordinates of the bottom of the bitmap.
			var bottom = top - quadHeight;
			*/

			// Calculate the screen coordinates of the left side of the bitmap.
			var left = (-1) + positionX*2;
			// Calculate the screen coordinates of the right side of the bitmap.
			var right = -1+(quadWidth+positionX)*2;
			// Calculate the screen coordinates of the top of the bitmap.
			var top = (-1) + positionY * 2;
			// Calculate the screen coordinates of the bottom of the bitmap.
			var bottom = -1 + (quadHeight + positionY) * 2;

			// Create and load the vertex array.
			List<UInt16> IndexBuffer = new List<UInt16>() { 0, 1, 2, 0, 3, 1 };
			List<ROD_core.Basic_structures.Vertex_POS_TEX> VertexBuffer = new List<Basic_structures.Vertex_POS_TEX>()
			{
				new Basic_structures.Vertex_POS_TEX
				(
					new Vector3(left, top, 0),
					new Vector2(0, 1)
				),
				new Basic_structures.Vertex_POS_TEX
				(
					new Vector3(right, bottom, 0),
					new Vector2(1, 0)
				),
				new Basic_structures.Vertex_POS_TEX
				(
					new Vector3(left, bottom, 0),
					new Vector2(0, 0)
				),
				new Basic_structures.Vertex_POS_TEX
				(
					new Vector3(right, top, 0),
					new Vector2(1, 1)
				)
			};

			mesh._indexStream = new IndexStream(IndexBuffer.Count, typeof(UInt16), true, true);
			mesh._vertexStream = new VertexStream(VertexBuffer.Count, true, true, vertexDefinition);
			foreach (UInt16 id in IndexBuffer)
			{
				mesh._indexStream.WriteIndex(id);
			}
			foreach (ROD_core.Basic_structures.Vertex_POS_TEX vd in VertexBuffer)
			{
				//object[] obj = new object[] { vd.pos, vd.normal, vd.UV, vd.binormal, vd.tangent };
				object[] obj = new object[] { vd.pos, vd.tex };
				mesh._vertexStream.WriteVertex(obj);
			}


			return true;
		}
		public new void Initialize(Device Device)
		{
			_shaderSolution = ROD_core.ShaderBinding.GetCompatibleShader(this);
			layout = new InputLayout(Device, _shaderSolution.shaders_bytecode[Shaders.VertexShader], mesh._vertexStream.vertexDefinition.GetInputElements());

			mesh.Load(Device);
			sampler = new SamplerState(Device, new SamplerStateDescription()
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				BorderColor = new SharpDX.Color4(0, 0, 0, 1),
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
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mesh.vertexBuffer, mesh._vertexStream.vertexType.SizeOf(), 0));
			context.InputAssembler.SetIndexBuffer(mesh.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
			context.VertexShader.Set(_shaderSolution.vs);
			context.PixelShader.Set(_shaderSolution.ps);
			context.HullShader.Set(null);
			context.DomainShader.Set(null);
			for(int i = 0; i<material.textures.Count; i++)
			{
				context.PixelShader.SetShaderResource(i, material.textures[i]);
			}
			context.PixelShader.SetSampler(0, sampler);
			context.DrawIndexed(mesh._indexStream.getIndexCount(), 0, 0);
			context.PixelShader.SetShaderResource(0, null);
		}
		#endregion
	}
}
