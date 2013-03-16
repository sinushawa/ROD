using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace ROD_core
{
    public class Scene
    {
        public List<Model> models;

        public Scene()
        {
        }

        public void Initialize()
        {
            models = new List<Model>();
        }

        public void Prep(Device Device)
        {
            foreach(Model _model in models)
            {
                _model.Initialize(Device);
            }
        }

        public void Render(DeviceContext context, vsBuffer vsBuffer, psBuffer psBuffer)
        {
            foreach (Model model in models)
            {
                model.Render(context, vsBuffer, psBuffer);
            }
        }
    }
}
