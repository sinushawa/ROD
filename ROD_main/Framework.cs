using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ROD_engine_DX11
{
    /// <summary>
    /// Data structure that represents a message sent from the OS to the program.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct Message
    {
        public IntPtr hWnd;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point p;
    }

    public abstract class Framework
    {
        // A function that lets us peeks at the first message avaliable from the OS
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

        // Checks is there are any messages avaliable from the OS
        private static bool IsIdle
        {
            get
            {
                Message msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        public Form Window { get; set; }

        public Factory Factory { get; set; }
        private bool VerticalSyncEnabled { get; set; }
        private bool _depth;

        public Device Device;
        public DeviceContext DContext;

        public SwapChain swapChain;
        public RenderTargetView RenderTargetView { get; set; }
        public DepthStencilView DepthStencilView { get; set; }
        public DepthStencilState DepthStencilState { get; private set; }
        public DepthStencilState DepthDisabledStencilState { get; private set; }

        public BlendState AlphaEnableBlendingState { get; private set; }
        public BlendState AlphaDisableBlendingState { get; private set; }

        public Stopwatch Stopwatch { get; set; }
        public SharpDX.Color4 Backcolor { get; set; }
        public SharpDX.DirectInput.Mouse mouse;
        public SharpDX.DirectInput.MouseState mouseState;
        public SharpDX.DirectInput.Keyboard keyboard;
        public SharpDX.DirectInput.KeyboardState keyboardState;

        protected Vector2 mouseCoord = new Vector2(0, 0);
        protected Vector2 mouseDelta = new Vector2(0, 0);
        protected int zoom = 0;

        public int frame_width;
        public int frame_height;
        

        public Framework(string title, int width, int height, bool depth, bool stencil, bool _VSync)
        {
            // Create the display to display on
            Window = new Form()
            {
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(width, height),
                Text = title,
                TopMost = true,
            };
            Window.StartPosition = FormStartPosition.Manual;
            Window.Location = new Point(1100, 80);
            Window.FormBorderStyle = FormBorderStyle.Fixed3D;
            Window.WindowState =FormWindowState.Normal;
            Window.CreateControl();
            VerticalSyncEnabled = _VSync;
            CreateDevice();

            // Create a description of the display mode
            var modeDescription = new ModeDescription()
            {
                Format = Format.R8G8B8A8_UNorm,
                RefreshRate = new Rational(60, 1),
                Scaling = DisplayModeScaling.Unspecified,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                Width = width,
                Height = height,
            };
            
            // Create a description of the sampling for multisampling or antialiasing
            var sampleDescription = new SampleDescription()
            {
                Count = 1,
                Quality = 0,
            };
            
            // Create a description of the swap chain or front and back buffers
            var swapDescription = new SwapChainDescription()
            {
                ModeDescription = modeDescription,
                SampleDescription = sampleDescription,
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                OutputHandle = Window.Handle,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };
            
            // Create the DirectX 11 Device
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport|DeviceCreationFlags.Debug, swapDescription, out Device, out swapChain);

            DContext = Device.ImmediateContext;
            // Create the factory which manages general graphics resources
            // Ignore all windows events
            Factory = swapChain.GetParent<Factory>();
            Factory.MakeWindowAssociation(Window.Handle, WindowAssociationFlags.IgnoreAll);
            Factory.Dispose();
            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            RenderTargetView = new RenderTargetView(Device, backBuffer);
            RenderTargetView.DebugName = "std";
            // Release pointer to the back buffer as we no longer need it.
            backBuffer.Dispose();
            
            frame_width=Window.ClientSize.Width;
            frame_height=Window.ClientSize.Height;

            #region Zdepth
            if (depth || stencil)
            {
                _depth=true;
                var textureDescription = new Texture2DDescription()
                {
                    Width = frame_width,
                    Height = frame_height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = (stencil) ? Format.D32_Float : Format.D24_UNorm_S8_UInt,
                    SampleDescription = sampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                };

                var texture = new Texture2D(Device, textureDescription);

                var depthStencilViewDescription = new DepthStencilViewDescription()
                {
                    Format = textureDescription.Format,
                    Dimension = DepthStencilViewDimension.Texture2DMultisampled
                };

                DepthStencilView = new DepthStencilView(Device, texture, depthStencilViewDescription);
                DContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
            }
            else
            {
                DContext.OutputMerger.SetTargets(RenderTargetView);
            }
            
            if (depth && !stencil)
                depthStencilClear = DepthStencilClearFlags.Depth;
            else if (stencil && !depth)
                depthStencilClear = DepthStencilClearFlags.Stencil;
            else if (stencil && depth)
                depthStencilClear = DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth;
            #endregion

            #region Rasterizer
            RasterizerStateDescription RAS = new RasterizerStateDescription();
            RAS.IsMultisampleEnabled = true; //important for AA
            RAS.CullMode = CullMode.Back;
            RAS.DepthBias = 0;
            RAS.DepthBiasClamp = 0.0f;
            RAS.FillMode = FillMode.Solid;
            RAS.IsDepthClipEnabled = false;
            RAS.IsFrontCounterClockwise = true;
            DContext.Rasterizer.State = new RasterizerState(Device, RAS);
            #endregion

            // Setup the camera viewport
            var viewport = new Viewport()
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };
            DContext.Rasterizer.SetViewports(viewport);

            Stopwatch = new Stopwatch();
        }
        public void TargetView_To_Screen_output()
        {
            if (_depth)
            {
                DContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
            }
            else
            {
                DContext.OutputMerger.SetTargets(RenderTargetView);
            }
        }

        private float last;
        private DepthStencilClearFlags depthStencilClear;

        private void ApplicationIdle(object sender, EventArgs e)
        {
            // While there are no system messages, keep rendering
            while (IsIdle)
            {
                float time = Stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                float delta = time - last;
                last = time;
                /*
                if (delta < 100)
                {
                    Thread.Sleep(10);
                }
                 * */
                MouseUpdate(time, delta);
                KeyboardUpdate(time, delta);
                Update(time, delta);

                // Clear or fill the render target with a single color
                DContext.ClearRenderTargetView(RenderTargetView, Backcolor);

                if (DepthStencilView != null)
                {
                    DContext.ClearDepthStencilView(DepthStencilView, depthStencilClear, 1.0f, 0);
                }

                Render(time, delta);
                if (VerticalSyncEnabled)
                {
                    swapChain.Present(1, PresentFlags.None);
                }
                else
                {
                    // Present as fast as possible.
                    swapChain.Present(0, PresentFlags.None);
                }
            }
        }

        public virtual void Dispose()
        {
            DContext.ClearState();
            Device.Dispose();

            // Dispose resources
            DepthStencilView.Dispose();
            RenderTargetView.Dispose();
            Device.Dispose();
            swapChain.Dispose();
            Factory.Dispose();
            Window.Dispose();
        }

        public void Run()
        {
            Stopwatch.Start();

            // When the application is idle (has handled all system messages) the event is raised
            Application.Idle += new EventHandler(ApplicationIdle);
            Application.Run(Window);
        }
        public void CreateDevice()
        {
            SharpDX.DirectInput.DirectInput dinput = new SharpDX.DirectInput.DirectInput();
            SharpDX.DirectInput.CooperativeLevel cooperativeLevel;
            cooperativeLevel = SharpDX.DirectInput.CooperativeLevel.NonExclusive;
            cooperativeLevel |= SharpDX.DirectInput.CooperativeLevel.Background;
            mouse = new SharpDX.DirectInput.Mouse(dinput);
            mouse.SetCooperativeLevel(Window, cooperativeLevel);
            mouse.Acquire();
            keyboard = new SharpDX.DirectInput.Keyboard(dinput);
            cooperativeLevel = SharpDX.DirectInput.CooperativeLevel.NonExclusive;
            cooperativeLevel |= SharpDX.DirectInput.CooperativeLevel.Foreground;
            keyboard.SetCooperativeLevel(Window, cooperativeLevel);
            keyboard.Acquire();
            Point startPoint = System.Windows.Forms.Cursor.Position;
            mouseCoord.X = Window.PointToClient(startPoint).X;
            mouseCoord.Y = Window.PointToClient(startPoint).Y;
            mouseState = new SharpDX.DirectInput.MouseState();
            keyboardState = new SharpDX.DirectInput.KeyboardState();            
        }

        protected abstract void MouseUpdate(float time, float step);
        protected abstract void KeyboardUpdate(float time, float step);
        protected abstract void Update(float time, float step);

        protected abstract void Render(float time, float step);
    }
}
