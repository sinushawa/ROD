using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ROD_core.Graphics.Assets;

namespace ROD_engine_DX11
{
    struct CameraSettings
    {
        public Vector3 eye;
        public Vector3 at;
        public Vector3 up;

        public CameraSettings(Vector3 e, Vector3 a, Vector3 u)
        {
            eye = e;
            at = a;
            up = u;
        }
    }

    public class ROD_Main : Framework
    {

        private Vector3 lightPos;
        private Quaternion lightRotation;
        private Vector4 lightColor;
        private Matrix world;
        private Matrix viewproj;
        private ROD_core.Scene scene;
        private ROD_core.RenderToTexture.RenderTexture render_texture;
        private ROD_core.RenderToTexture.ScreenQuad sq;

        public ROD_core.Camera camera;

        public ROD_Main() : base("FrameDX", 1280, 800, true, false, true)
        {
            #region HLSL definition          

            //Shader for diffuse texture, normal texture and bump with tesselation
            ROD_core.ByteCodeBind[] ShadersByteCodeDNT = new ROD_core.ByteCodeBind[]{
                new ROD_core.ByteCodeBind(ROD_core.Shaders.VertexShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation.vs", "VS", "vs_5_0",ShaderFlags.Debug)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.HullShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation.hs", "HS", "hs_5_0",ShaderFlags.Debug)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.DomainShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation.ds", "DS", "ds_5_0",ShaderFlags.Debug)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.PixelShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation_ward.ps", "PS", "ps_5_0",ShaderFlags.Debug))
            };
            ROD_core.ShaderSolution ShSolutionDNT = new ROD_core.ShaderSolution("DNS_Tes", Device, ShadersByteCodeDNT);
            ROD_core.Technique DNT = ROD_core.Technique.Diffuse_mapping|ROD_core.Technique.Normal_mapping|ROD_core.Technique.Specular_mapping|ROD_core.Technique.Tesslation;

            ROD_core.ShaderBinding.ShaderPool.Add(DNT, ShSolutionDNT);


            //Shader for simple diffuse texture
            ROD_core.ByteCodeBind[] ShadersByteCodeD = new ROD_core.ByteCodeBind[]{
                new ROD_core.ByteCodeBind(ROD_core.Shaders.VertexShader, ShaderBytecode.CompileFromFile(@"shaders\Diffuse.vs", "VS", "vs_5_0")),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.PixelShader, ShaderBytecode.CompileFromFile(@"shaders\Diffuse.ps", "PS", "ps_5_0", ShaderFlags.SkipOptimization))
            };
            ROD_core.ShaderSolution ShSolutionD = new ROD_core.ShaderSolution("D", Device, ShadersByteCodeD);
            ROD_core.Technique D = ROD_core.Technique.Diffuse_mapping;

            ROD_core.ShaderBinding.ShaderPool.Add(D, ShSolutionD);


            //Shader for screen quad diffuse texture
            ROD_core.ByteCodeBind[] ShadersByteCodeSQ = new ROD_core.ByteCodeBind[]{
                new ROD_core.ByteCodeBind(ROD_core.Shaders.VertexShader, ShaderBytecode.CompileFromFile(@"shaders\Diffuse2D.vs", "VS", "vs_5_0")),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.PixelShader, ShaderBytecode.CompileFromFile(@"shaders\Diffuse2D.ps", "PS", "ps_5_0"))
            };
            ROD_core.ShaderSolution ShSolutionSQ = new ROD_core.ShaderSolution("SQ", Device, ShadersByteCodeSQ);
            ROD_core.Technique SQ = ROD_core.Technique.Quad_rendering;

            ROD_core.ShaderBinding.ShaderPool.Add(SQ, ShSolutionSQ);
            SetEnvironnement();
            SetShaders();
            InitializeScene();
            scene.Prep(Device);
            
            

            #endregion
        }

        private void InitializeScene()
        {
            scene = new ROD_core.Scene();
            scene.Initialize();

            render_texture = new ROD_core.RenderToTexture.RenderTexture();
            bool render_target_initialization_result = render_texture.Initialize(Device, frame_width, frame_height);
            sq = new ROD_core.RenderToTexture.ScreenQuad();
            sq.CreateMesh(Device, 1.0f, 1.0f);
            

            // material definition
            Material body_material = new Material("body_mat");
            body_material.maps.Add(MapSlot.Diffuse, "AsManBodyD.jpg");
            body_material.maps.Add(MapSlot.Normal, "AsManBodyN.jpg");
            body_material.maps.Add(MapSlot.Specular, "AsManBodyS.jpg");

            Material head_material = new Material("head_mat");
            head_material.maps.Add(MapSlot.Diffuse, "AsManHeadD.jpg");
            head_material.maps.Add(MapSlot.Normal, "AsManHeadN.jpg");
            head_material.maps.Add(MapSlot.Specular, "AsManHeadS.jpg");

            Material hair_material = new Material("hair_mat");
            hair_material.maps.Add(MapSlot.Diffuse, "AsManHairD.jpg");
            hair_material.maps.Add(MapSlot.Normal, "AsManHairN.jpg");
            hair_material.maps.Add(MapSlot.Specular, "AsManHairS.jpg");

            Material bricks_material = new Material("bricks_mat");
            bricks_material.maps.Add(MapSlot.Diffuse, "JP_Brick01_Bump.jpg");

            // !!!!!!!!! Material used to render texture to backbuffer
            Material Render_TO_Tex_material = new Material("RTT_mat");
            Render_TO_Tex_material.maps.Add(MapSlot.Deferred, "");
            Render_TO_Tex_material.textures.Add(render_texture.ShaderResourceView);

            //

            Mesh body_mesh = Mesh.createFromFile("bodyBB.rod");
            Model body = new Model(body_mesh, body_material, true);
            Mesh head_mesh = Mesh.createFromFile("headBB.rod");
            Model head = new Model(head_mesh, head_material, true);
            Mesh eyes_mesh = Mesh.createFromFile("eyesBB.rod");
            Model eyes = new Model(eyes_mesh, head_material, true);
            Mesh hair_mesh = Mesh.createFromFile("hairBB.rod");
            Model hair = new Model(hair_mesh, hair_material, true);
            //Mesh box_mesh = Mesh.createFromFile("boxBB.rod");
            //Model box = new Model(box_mesh, bricks_material, false);

            scene.models.Add(body);
            scene.models.Add(head);
            scene.models.Add(eyes);
            scene.models.Add(hair);
            //scene.models.Add(box);

            sq.material = Render_TO_Tex_material;
            sq.Initialize(Device);
        }

        private void SetEnvironnement()
        {
            // Set up the camera
            Vector3 eye = new Vector3(0.0f, 100.0f, -300.0f);   // Where the camera is looking from
            Vector3 target = new Vector3(0.0f, 100.0f, 0.0f);     // Where the camera is looking at
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);     // Vector point upwards
            camera = new ROD_core.Camera(eye, target);
            camera.CreateProjection(55.0f, Window.ClientSize.Width, Window.ClientSize.Height, 0.1f, 2000.0f);
            lightPos = new Vector3(0.0f, 100.0f, -300.0f);
            lightRotation = Quaternion.Identity;
            lightColor = new Vector4(1f, 1f, 1f, 1.0f);
            world = Matrix.Identity;

            
        }
        private void SetShaders()
        {
            ROD_core.ShaderBinding.ConstantsPool.Add("World", new ROD_core.Constant_Variable<Matrix>(ref world));
            ROD_core.ShaderBinding.ConstantsPool.Add("ViewProjection", new ROD_core.Constant_Variable<Matrix>(ref viewproj));
            ROD_core.ShaderBinding.ConstantsPool.Add("eyePos", new ROD_core.Constant_Variable<Vector3>(ref camera.cameraTransformed.eye));
            ROD_core.ShaderBinding.ConstantsPool.Add("LightPos", new ROD_core.Constant_Variable<Vector3>(ref lightPos));
            ROD_core.ShaderBinding.ConstantsPool.Add("LightColor", new ROD_core.Constant_Variable<Vector4>(ref lightColor));

            ROD_core.ShaderBinding.BuildBuffers(Device);
            ROD_core.ShaderBinding.InitConstants();
        }

        public override void Dispose()
        {
            scene.Dispose();
            sq.Dispose();
            base.Dispose();
        }
        protected override void MouseUpdate(float time, float step)
        {
            mouse.Poll();
            mouse.GetCurrentState(ref mouseState);
            if (mouseState.Buttons[1])
            {
                mouseDelta.X = mouseState.X;
                mouseDelta.Y = mouseState.Y;
                camera.Orbit((mouseDelta.X / 5), (mouseDelta.Y / 5));
            }
            else
            {
                mouseDelta.X = 0;
                mouseDelta.Y = 0;
                mouseCoord.X += mouseState.X;
                mouseCoord.Y += mouseState.Y;
                _mouseWheelFactor = mouseState.Z;
                camera.Zoom(_mouseWheelFactor/5.0f);
            }

        }
        protected override void KeyboardUpdate(float time, float step)
        {
            try
            {
                keyboard.Poll();
                keyboard.GetCurrentState(ref keyboardState);
                List<SharpDX.DirectInput.Key> pressed_keys = keyboardState.PressedKeys;
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Up))
                {
                    camera.Pan(0.0f, 5.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Down))
                {
                    camera.Pan(0.0f, -5.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.D))
                {
                    camera.Pan(5.0f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.A))
                {
                    camera.Pan(-5.0f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Q))
                {
                    camera.Revolve(1.0f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.E))
                {
                    camera.Revolve(-1.0f, 0.0f);
                }
            }
            catch
            {
                
            }
        }

        protected override void Update(float time, float step)
        {
            Debug.WriteLine(time);
            camera.Update();
            viewproj = Matrix.Multiply(camera.GetViewMatrix(), camera.projection);
            viewproj.Transpose();
            float rotAngle = ROD_core.Mathematics.Math_helpers.ToRadians(0.025f * step/1000);
            Quaternion rotLight = Quaternion.RotationAxis(Vector3.UnitY, rotAngle);
            lightRotation = rotLight * lightRotation;
            lightPos = Vector3.TransformCoordinate(lightPos, Matrix.RotationQuaternion(lightRotation));

            object sent = ((object)viewproj);
            ROD_core.ShaderBinding.ConstantsPool["ViewProjection"].Update(ref sent);
            ROD_core.ShaderBinding.UpdateConstants("ViewProjection");
        }

        protected override void Render(float time, float step)
        {
            //render_texture.SetRenderTarget(DContext, DepthStencilView);

            // Bind the render target view and depth stencil buffer to the output pipeline.
            DContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
            // Setup the color the buffer to.
            var color = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            // Clear the render to texture buffer.
            DContext.ClearRenderTargetView(RenderTargetView, color);
            // Clear the depth buffer.
            DContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            scene.Render(DContext);
            //render_texture.SaveToFile(DContext, @"C:\test\test.jpg");
            
            
            TargetView_To_Screen_output();
            //sq.Render(DContext);
        }
    }
}
