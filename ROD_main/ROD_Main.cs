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

        private Vector3 LightPos;
        private Vector4 LightColor;
        private CameraSettings camset_default;
        private CameraSettings camset_transformed;
        private Matrix world;
        private Matrix view;
        private Matrix projection;
        private Matrix viewproj;
        private ROD_core.Scene scene;
        private ROD_core.RenderToTexture.RenderTexture render_texture;
        private ROD_core.RenderToTexture.ScreenQuad sq;

        private float accumaltedYaw = 0;
        private float accumaltedPitch = 0;

        public ROD_Main()
            : base("FrameDX", 1280, 800, true, false, true)
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

            InitializeScene();
            scene.Prep(Device);
            
            SetEnvironnement();

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
            ROD_core.Material body_material = new ROD_core.Material("body_mat");
            body_material.maps.Add(ROD_core.MapSlot.Diffuse, "AsManBodyD.jpg");
            body_material.maps.Add(ROD_core.MapSlot.Normal, "AsManBodyN.jpg");
            body_material.maps.Add(ROD_core.MapSlot.Specular, "AsManBodyS.jpg");

            ROD_core.Material head_material = new ROD_core.Material("head_mat");
            head_material.maps.Add(ROD_core.MapSlot.Diffuse, "AsManHeadD.jpg");
            head_material.maps.Add(ROD_core.MapSlot.Normal, "AsManHeadN.jpg");
            head_material.maps.Add(ROD_core.MapSlot.Specular, "AsManHeadS.jpg");

            ROD_core.Material hair_material = new ROD_core.Material("hair_mat");
            hair_material.maps.Add(ROD_core.MapSlot.Diffuse, "AsManHairD.jpg");
            hair_material.maps.Add(ROD_core.MapSlot.Normal, "AsManHairN.jpg");
            hair_material.maps.Add(ROD_core.MapSlot.Specular, "AsManHairS.jpg");

            ROD_core.Material bricks_material = new ROD_core.Material("bricks_mat");
            bricks_material.maps.Add(ROD_core.MapSlot.Diffuse, "JP_Brick01_Bump.jpg");

            // !!!!!!!!! Material used to render texture to backbuffer
            ROD_core.Material Render_TO_Tex_material = new ROD_core.Material("RTT_mat");
            Render_TO_Tex_material.maps.Add(ROD_core.MapSlot.Deferred, "");
            Render_TO_Tex_material.textures.Add(render_texture.ShaderResourceView);

            //

            ROD_core.Mesh body_mesh = ROD_core.Mesh.createFromFile("body.rod");
            ROD_core.Model body = new ROD_core.Model(body_mesh, body_material, true);
            ROD_core.Mesh head_mesh = ROD_core.Mesh.createFromFile("head.rod");
            ROD_core.Model head = new ROD_core.Model(head_mesh, head_material, true);
            ROD_core.Mesh eyes_mesh = ROD_core.Mesh.createFromFile("eyes.rod");
            ROD_core.Model eyes = new ROD_core.Model(eyes_mesh, head_material, true);
            ROD_core.Mesh hair_mesh = ROD_core.Mesh.createFromFile("hair.rod");
            ROD_core.Model hair = new ROD_core.Model(hair_mesh, hair_material, true);
            ROD_core.Mesh box_mesh = ROD_core.Mesh.createFromFile("box.rod");
            ROD_core.Model box = new ROD_core.Model(box_mesh, bricks_material, false);

            scene.models.Add(body);
            scene.models.Add(head);
            scene.models.Add(eyes);
            scene.models.Add(hair);
            scene.models.Add(box);

            sq.material = Render_TO_Tex_material;
            sq.Initialize(Device);
        }

        private void SetEnvironnement()
        {
            // Set up the camera
            Vector3 eye = new Vector3(0.0f, 100.0f, -300.0f);   // Where the camera is looking from
            Vector3 at = new Vector3(0.0f, 100.0f, 0.0f);     // Where the camera is looking at
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);     // Vector point upwards
            camset_default = new CameraSettings(eye, at, up);
            camset_transformed = new CameraSettings(eye, at, up);
            LightPos = new Vector3(0.0f, 100.0f, -300.0f);
            LightColor = new Vector4(1f, 1f, 1f, 1.0f);
            world = Matrix.Identity;

            /*
            Quaternion RotLight = Quaternion.RotationAxis(Vector3.UnitY, (ROD_core.Mathematics.Math_helpers.ToRadians(90)));
            LightPos = Vector3.TransformCoordinate(LightPos, Matrix.RotationQuaternion(RotLight));
             * */
        }

        public override void Dispose()
        {

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
            }
            else
            {
                mouseDelta.X = 0;
                mouseDelta.Y = 0;
                mouseCoord.X += mouseState.X;
                mouseCoord.Y += mouseState.Y;
                zoom = mouseState.Z * 5;
            }

        }
        protected override void KeyboardUpdate(float time, float step)
        {
            try
            {
                keyboard.Poll();
                keyboard.GetCurrentState(ref keyboardState);
                List<SharpDX.DirectInput.Key> pressed_keys = keyboardState.PressedKeys;
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.UpArrow))
                {
                    camset_default.eye.Y += 5.0f;
                    camset_default.at.Y += 5.0f;
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Down))
                {
                    camset_default.eye.Y -= 5.0f;
                    camset_default.at.Y -= 5.0f;
                }
            }
            catch
            {
                
            }
        }

        protected override void Update(float time, float step)
        {
            Vector3 LookAt = Vector3.Normalize(camset_default.at - camset_default.eye);
            camset_default.eye += (Vector3.Multiply(LookAt, ((float)zoom) / 100));
            if (mouseDelta.X > -1 && mouseDelta.X < 1)
            {
                mouseDelta.X = 0;
            }
            if (mouseDelta.Y > -1 && mouseDelta.Y < 1)
            {
                mouseDelta.Y = 0;
            }
            accumaltedYaw += ROD_core.Mathematics.Math_helpers.ToRadians((mouseDelta.X / 5));
            accumaltedPitch += ROD_core.Mathematics.Math_helpers.ToRadians((mouseDelta.Y / 5));
            Quaternion RotYaw = Quaternion.RotationAxis(Vector3.UnitY, accumaltedYaw);
            Vector3 AxisPitch = Vector3.TransformCoordinate(Vector3.UnitX, Matrix.RotationQuaternion(RotYaw));
            Quaternion RotPitch = Quaternion.RotationAxis(AxisPitch, accumaltedPitch);
            Quaternion RotFinal = RotYaw * RotPitch;
            Vector3 transformedEye = Vector3.TransformCoordinate(camset_default.eye, Matrix.RotationQuaternion(RotFinal));
            Vector3 transformedUp = Vector3.TransformCoordinate(camset_default.up, Matrix.RotationQuaternion(RotFinal));
            camset_transformed.eye = transformedEye;
            view = Matrix.LookAtLH(transformedEye, camset_default.at, transformedUp);
            projection = Matrix.PerspectiveFovLH(ROD_core.Mathematics.Math_helpers.ToRadians(55.0f), Window.ClientSize.Width / (float)Window.ClientSize.Height, 0.1f, 2000.0f);
            viewproj = Matrix.Multiply(view, projection);
            viewproj.Transpose();
            Quaternion RotLight = Quaternion.RotationAxis(Vector3.UnitY, ((step / 15) * ROD_core.Mathematics.Math_helpers.ToRadians(360)));
            LightPos = Vector3.TransformCoordinate(LightPos, Matrix.RotationQuaternion(RotLight));
        }

        protected override void Render(float time, float step)
        {
            ROD_core.vsBuffer vsBuffer = new ROD_core.vsBuffer();
            vsBuffer.padding3 = 0;
            vsBuffer.padding1 = 0;
            vsBuffer.World = world;
            vsBuffer.ViewProjection = viewproj;
            vsBuffer.eyePos = camset_transformed.eye;
            vsBuffer.LightPos = LightPos;
            ROD_core.psBuffer psBuffer = new ROD_core.psBuffer();
            psBuffer.LightColor = LightColor;
            render_texture.SetRenderTarget(DContext, DepthStencilView);
            scene.Render(DContext, vsBuffer, psBuffer);
            //render_texture.SaveToFile(DContext, @"C:\test\test.jpg");
            
            
            TargetView_To_Screen_output();
            sq.Render(DContext);
        }

    }
}
