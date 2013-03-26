using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace ROD_core.RenderToTexture
{
    public class RenderTexture : Component
    {
        #region Variables / Properties
        private Texture2D texRenderTargetTexture { get; set; }
        private Texture2D extRenderTargetTexture { get; set; }
        public RenderTargetView texRenderTargetView { get; set; }
        public ShaderResourceView ShaderResourceView { get; private set; }
        #endregion

        #region Public Methods
        public bool Initialize(Device device, int _width, int _height)
        {
            try
            {
                ToDispose(texRenderTargetTexture);
                ToDispose(extRenderTargetTexture);
                // Initialize and set up the render target description.
                var textureDesc = new Texture2DDescription()
                {
                    Width = _width,
                    Height = _height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R32G32B32A32_Float,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };
                /*
                var exttextureDesc = new Texture2DDescription()
                {
                    Width = _width,
                    Height = _height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R8G8B8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Staging,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.Shared,

                };
                 * */


                // Create the render target texture.
                texRenderTargetTexture = new Texture2D(device, textureDesc);
                //extRenderTargetTexture = new Texture2D(device, exttextureDesc);

                // Initialize and setup the render target view 
                var renderTargetViewDesc = new RenderTargetViewDescription()
                {
                    Format = textureDesc.Format,
                    Dimension = RenderTargetViewDimension.Texture2D,
                };
                renderTargetViewDesc.Texture2D.MipSlice = 0;

                // Create the render target view.
                texRenderTargetView = new RenderTargetView(device, texRenderTargetTexture, renderTargetViewDesc);
                texRenderTargetView.DebugName = "text";

                // Initialize and setup the shader resource view 
                var shaderResourceViewDesc = new ShaderResourceViewDescription()
                {
                    Format = textureDesc.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                };
                shaderResourceViewDesc.Texture2D.MipLevels = 1;
                shaderResourceViewDesc.Texture2D.MostDetailedMip = 0;

                // Create the shader resource view.
                ShaderResourceView = new ShaderResourceView(device, texRenderTargetTexture, shaderResourceViewDesc);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void Shutdown()
        {
            if (ShaderResourceView != null)
            {
                ShaderResourceView.Dispose();
                ShaderResourceView = null;
            }

            if (texRenderTargetView != null)
            {
                texRenderTargetView.Dispose();
                texRenderTargetView = null;
            }

            if (texRenderTargetTexture != null)
            {
                texRenderTargetTexture.Dispose();
                texRenderTargetTexture = null;
            }
        }

        public void SetRenderTarget(DeviceContext context, DepthStencilView depthStencilView)
        {
            // Bind the render target view and depth stencil buffer to the output pipeline.
            context.OutputMerger.SetTargets(depthStencilView, texRenderTargetView);

            // Setup the color the buffer to.
            var color = new Color4(0.0f, 0.0f, 1.0f, 1.0f);

            // Clear the render to texture buffer.
            context.ClearRenderTargetView(texRenderTargetView, color);

            // Clear the depth buffer.
            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void SaveToFile(DeviceContext context, string path)
        {
            context.CopyResource(texRenderTargetTexture, extRenderTargetTexture);
            SharpDX.Direct3D11.Resource.ToFile(context, extRenderTargetTexture, ImageFileFormat.Jpg, path);
        }

        #endregion
    }
}
