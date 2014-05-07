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
using ROD_core.Graphics.Animation;
using ROD_core.Mathematics;
using Assimp;
using Quaternion = SharpDX.Quaternion;
using Material = ROD_core.Graphics.Assets.Material;
using Mesh = ROD_core.Graphics.Assets.Mesh;
using ROD_core.Mathematics.Conversions.Assimp;

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
        private Skeleton skelete;
        private HierarchicalJoint root;
        private ROD_core.Scene scene;
        private ROD_core.RenderToTexture.RenderTexture render_texture;
        private ROD_core.RenderToTexture.ScreenQuad sq;

        public ROD_core.Camera camera;

        public ROD_Main() : base("FrameDX", 1280, 800, true, false, false)
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


            //Shader for diffuse texture, normal texture and bump with tesselationand skinning
            ROD_core.ByteCodeBind[] ShadersByteCodeDNTS = new ROD_core.ByteCodeBind[]{
                new ROD_core.ByteCodeBind(ROD_core.Shaders.VertexShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselationSkinning.vs", "VS", "vs_5_0",ShaderFlags.Debug|ShaderFlags.SkipOptimization)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.HullShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation.hs", "HS", "hs_5_0",ShaderFlags.Debug)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.DomainShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation.ds", "DS", "ds_5_0",ShaderFlags.Debug)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.PixelShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalTesselation_ward.ps", "PS", "ps_5_0",ShaderFlags.Debug))
            };
            ROD_core.ShaderSolution ShSolutionDNTS = new ROD_core.ShaderSolution("DNS_TesS", Device, ShadersByteCodeDNTS);
            ROD_core.Technique DNTS = ROD_core.Technique.Diffuse_mapping | ROD_core.Technique.Normal_mapping | ROD_core.Technique.Specular_mapping | ROD_core.Technique.Tesslation | ROD_core.Technique.Skinning;


            ROD_core.ShaderBinding.ShaderPool.Add(DNTS, ShSolutionDNTS);

            //Shader for diffuse texture, normal texture and bump with tesselationand skinning
            ROD_core.ByteCodeBind[] ShadersByteCodeDNS = new ROD_core.ByteCodeBind[]{
                new ROD_core.ByteCodeBind(ROD_core.Shaders.VertexShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormalSkinning.vs", "VS", "vs_5_0",ShaderFlags.Debug|ShaderFlags.SkipOptimization)),
                new ROD_core.ByteCodeBind(ROD_core.Shaders.PixelShader, ShaderBytecode.CompileFromFile(@"shaders\DiffuseNormal.ps", "PS", "ps_5_0",ShaderFlags.Debug))
            };
            ROD_core.ShaderSolution ShSolutionDNS = new ROD_core.ShaderSolution("DNS_S", Device, ShadersByteCodeDNS);
            ROD_core.Technique DNS = ROD_core.Technique.Diffuse_mapping | ROD_core.Technique.Normal_mapping | ROD_core.Technique.Specular_mapping | ROD_core.Technique.Skinning;


            ROD_core.ShaderBinding.ShaderPool.Add(DNS, ShSolutionDNS);


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

            AssimpImporter importer = new AssimpImporter();
            Scene model = importer.ImportFile("AsMan.DAE", PostProcessPreset.TargetRealTimeMaximumQuality);
            int bonesNb = model.Meshes[0].Bones.Length;
            List<Joint> readable = new List<Joint>();
            Node[] childs = model.RootNode.Children;
            for (int i = 0; i < bonesNb; i++)
            {
                Assimp.Quaternion _q = new Assimp.Quaternion();
                Assimp.Vector3D _t = new Vector3D();
                model.Meshes[0].Bones[i].OffsetMatrix.DecomposeNoScaling(out _q, out _t);
                DualQuaternion DQ = new DualQuaternion(_q.ConvertTo(), _t.ConvertTo());
                Joint _bindJoint = new Joint(i, model.Meshes[0].Bones[i].Name);
                _bindJoint.worldRotationTranslation = DQ;
                readable.Add(_bindJoint);
            }
            root = AssimpSkeleton.ConstructSkeleton(childs, null, readable);

            Vector3 point1 = new Vector3(5, 0, 0);
            Vector3 point2 = new Vector3(5, 2, 0);
            Vector3 point3 = new Vector3(5, 5, 0);
            Vector3 point4 = new Vector3(8, 5, 0);
            Vector3 axisZ = Vector3.UnitZ;
            Vector3 origin = new Vector3(0, 0, 0);
            DualQuaternion origin_to_pivot_transform1 = new DualQuaternion(Quaternion.Identity, origin);
            DualQuaternion origin_to_pivot_transform2 = new DualQuaternion(Quaternion.Identity, point1);
            DualQuaternion origin_to_pivot_transform3 = new DualQuaternion(Quaternion.Identity, point2);
            DualQuaternion origin_to_pivot_transform4 = new DualQuaternion(Quaternion.Identity, point3);
            Quaternion dq1 = Quaternion.RotationAxis(axisZ, Math_helpers.ToRadians(90));
            Quaternion dq2 = Quaternion.RotationAxis(axisZ, -Math_helpers.ToRadians(90));
            Quaternion dq3 = Quaternion.RotationAxis(axisZ, Math_helpers.ToRadians(90));
            Quaternion dq4 = Quaternion.RotationAxis(axisZ, Math_helpers.ToRadians(90));
            Vector3 translation = new Vector3(0, 0, 0);
            DualQuaternion LocalTransform1 = new DualQuaternion(dq1, translation);
            DualQuaternion WorldTransform1 = DualQuaternion.Conjugate(origin_to_pivot_transform1) * LocalTransform1 * origin_to_pivot_transform1;
            DualQuaternion LocalTransform2 = new DualQuaternion(dq2, translation);
            DualQuaternion WorldTransform2 = DualQuaternion.Conjugate(origin_to_pivot_transform2) * LocalTransform2 * origin_to_pivot_transform2;
            DualQuaternion LocalTransform3 = new DualQuaternion(dq3, translation);
            DualQuaternion WorldTransform3 = DualQuaternion.Conjugate(origin_to_pivot_transform3) * LocalTransform3 * origin_to_pivot_transform3;
            DualQuaternion LocalTransform4 = new DualQuaternion(dq4, translation);
            DualQuaternion WorldTransform4 = DualQuaternion.Conjugate(origin_to_pivot_transform4) * LocalTransform4 * origin_to_pivot_transform4;
            Vector3 point_in_pivot_space1 = point1.TransformByDQ(DualQuaternion.Conjugate(origin_to_pivot_transform1));
            Vector3 transformed_point_in_pivot_space1 = point_in_pivot_space1.TransformByDQ(LocalTransform1);
            Vector3 transformed_point1 = transformed_point_in_pivot_space1.TransformByDQ(origin_to_pivot_transform1);
            Vector3 point_in_pivot_space2 = point2.TransformByDQ(DualQuaternion.Conjugate(origin_to_pivot_transform2));
            Vector3 transformed_point_in_pivot_space2 = point_in_pivot_space2.TransformByDQ(LocalTransform2);
            Vector3 transformed_point2 = transformed_point_in_pivot_space2.TransformByDQ(origin_to_pivot_transform2);
            DualQuaternion Oo = WorldTransform4 * DualQuaternion.Conjugate(WorldTransform3 * WorldTransform2 * WorldTransform1);
            Vector3 all_in_one = point4.TransformByDQ(WorldTransform4*WorldTransform3*WorldTransform2*WorldTransform1);
            Vector3 back = all_in_one.TransformByDQ(DualQuaternion.Conjugate(WorldTransform4 * WorldTransform3 * WorldTransform2 * WorldTransform1));
            /*
            float ticksPerSecond = (float)model.Animations[0].TicksPerSecond;
            int sampling = 30;
            List<HierarchicalJoint> skeletonJoints = root.ToList();
            for (int i = 0; i < model.Animations[0].NodeAnimationChannelCount; i++)
            {
                string name = model.Animations[0].NodeAnimationChannels[i].NodeName;
                HierarchicalJoint currentJoint = skeletonJoints.FirstOrDefault(x => x.name == model.Animations[0].NodeAnimationChannels[i].NodeName);
                if (currentJoint != null)
                {
                    Quaternion _q = Quaternion.Identity;
                    Vector3 _v = Vector3.Zero;
                    int step = model.Animations[0].NodeAnimationChannels[i].RotationKeys.Length / sampling;
                    for (int j = 0; j <= step; j++)
                    {
                        _q = model.Animations[0].NodeAnimationChannels[i].RotationKeys[j*sampling].Value.ConvertTo();
                        _v = model.Animations[0].NodeAnimationChannels[i].PositionKeys[j * sampling].Value.ConvertTo();
                    }
                    DualQuaternion DQ = new DualQuaternion(_q, _v);
                    currentJoint.localRotationTranslation = DQ;
                }
            }
             */
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

            Mesh body_mesh = Mesh.createFromFile("testBB.rod");
            Entity body = new Entity(body_mesh, body_material, false);
            /*
            Mesh body_mesh = Mesh.createFromFile("bodyBB.rod");
            Model body = new Model(body_mesh, body_material, true);
            Mesh head_mesh = Mesh.createFromFile("headBB.rod");
            Model head = new Model(head_mesh, head_material, true);
            Mesh eyes_mesh = Mesh.createFromFile("eyesBB.rod");
            Model eyes = new Model(eyes_mesh, head_material, true);
            Mesh hair_mesh = Mesh.createFromFile("hairBB.rod");
            Model hair = new Model(hair_mesh, hair_material, true);
             * */
            //Mesh box_mesh = Mesh.createFromFile("boxBB.rod");
            //Model box = new Model(box_mesh, bricks_material, false);

            scene.models.Add(body);
            /*
            scene.models.Add(body);
            scene.models.Add(head);
            scene.models.Add(eyes);
            scene.models.Add(hair);
             * */
            //scene.models.Add(box);


            sq.material = Render_TO_Tex_material;
            sq.Initialize(Device);
        }

        private void SetEnvironnement()
        {
            // Set up the camera
            Vector3 eye = new Vector3(0.0f, 1.0f, -3.0f);   // Where the camera is looking from
            Vector3 target = new Vector3(0.0f, 1.0f, 0.0f);     // Where the camera is looking at
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);     // Vector point upwards

            Vector3 axisX = Vector3.UnitX;
            Quaternion DX_to_MAX_dq = Quaternion.RotationAxis(axisX, Math_helpers.ToRadians(90));
            Matrix DX_to_MAX_mx = Matrix.RotationQuaternion(DX_to_MAX_dq);
            eye = Vector3.TransformCoordinate(eye, DX_to_MAX_mx);
            target = Vector3.TransformCoordinate(target, DX_to_MAX_mx);
            up = Vector3.TransformCoordinate(up, DX_to_MAX_mx);
            camera = new ROD_core.Camera(eye, target, up);
            camera.CreateProjection(55.0f, Window.ClientSize.Width, Window.ClientSize.Height, 0.001f, 200.0f);
            lightPos = new Vector3(0.0f, 1.0f, -3.0f);
            lightRotation = Quaternion.Identity;
            lightColor = new Vector4(1f, 1f, 1f, 1.0f);
            world = Matrix.Identity;

            Clip_Skinning clip = Clip_Skinning.createFromFile("testBB.clp");
            clip.Init();
            clip.isPlaying = true;
            skelete = Skeleton.createFromFile("testBB.skl");
            skelete.animation.clips.Add(clip);
            skelete.animation.clipWeights.Add(1);
            
            
        }
        private void SetShaders()
        {
            ROD_core.ShaderBinding.ConstantsPool.Add("World", new ROD_core.Constant_Variable<Matrix>(ref world));
            ROD_core.ShaderBinding.ConstantsPool.Add("ViewProjection", new ROD_core.Constant_Variable<Matrix>(ref viewproj));
            ROD_core.ShaderBinding.ConstantsPool.Add("eyePos", new ROD_core.Constant_Variable<Vector3>(ref camera.cameraTransformed.eye));
            ROD_core.ShaderBinding.ConstantsPool.Add("LightPos", new ROD_core.Constant_Variable<Vector3>(ref lightPos));
            ROD_core.ShaderBinding.ConstantsPool.Add("LightColor", new ROD_core.Constant_Variable<Vector4>(ref lightColor));
            ROD_core.ShaderBinding.ConstantsPool.Add("BoneDQ", new ROD_core.Constant_Variable<DualQuaternion>(ref skelete.BonePalette));

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
                camera.Orbit((mouseDelta.X / 10), (mouseDelta.Y / 10));
            }
            else
            {
                mouseDelta.X = 0;
                mouseDelta.Y = 0;
                mouseCoord.X += mouseState.X;
                mouseCoord.Y += mouseState.Y;
                _mouseWheelFactor = mouseState.Z;
                camera.Zoom(_mouseWheelFactor/5000.0f);
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
                    camera.Pan(0.0f, 0.005f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Down))
                {
                    camera.Pan(0.0f, -0.005f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.D))
                {
                    camera.Pan(0.005f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.A))
                {
                    camera.Pan(-0.005f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.Q))
                {
                    camera.Revolve(0.001f, 0.0f);
                }
                if (keyboardState.IsPressed(SharpDX.DirectInput.Key.E))
                {
                    camera.Revolve(-0.001f, 0.0f);
                }
            }
            catch
            {
                
            }
        }

        protected override void Update(float time, float step)
        {
            skelete.Update(step);
            camera.Update();
            viewproj = Matrix.Multiply(camera.GetViewMatrix(), camera.projection);
            viewproj.Transpose();
            float rotAngle = ROD_core.Mathematics.Math_helpers.ToRadians(0.025f * step/1);
            Quaternion rotLight = Quaternion.RotationAxis(Vector3.UnitZ, rotAngle);
            lightRotation = rotLight * lightRotation;
            Vector3 ElightPos = Vector3.TransformCoordinate(lightPos, Matrix.RotationQuaternion(lightRotation));
            object sent = ((object)viewproj);
            ROD_core.ShaderBinding.ConstantsPool["ViewProjection"].Update(ref sent);
            object sentlp = ((object)ElightPos);
            ROD_core.ShaderBinding.ConstantsPool["LightPos"].Update(ref sentlp);
            object sentbm = ((object)skelete.BonePalette);
            ROD_core.ShaderBinding.ConstantsPool["BoneDQ"].Update(ref sentbm);
            ROD_core.ShaderBinding.UpdateFlaggedConstants();
            //ROD_core.ShaderBinding.UpdateConstants("ViewProjection");
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
