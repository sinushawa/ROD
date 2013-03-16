using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using Device=SharpDX.Direct3D11.Device;

namespace ROD_core
{
    [Flags]
    public enum MapSlot : long
    {
        None = (1 << 0),
        Diffuse = (1 << 1),
        Specular = (1 << 2),
        Normal = (1 << 3),
        Heightmap = (1 << 4),
        Reflection = (1 << 5),
        Opacity = (1 << 6),
        Deferred = (1 << 13)
    }

    public class Material
    {
        public string name;
        public Guid ID;
        public Dictionary<MapSlot, string> maps;
        public List<ShaderResourceView> textures;

        public Material(string _name)
        {
            name = _name;
            ID = Guid.NewGuid();
            maps = new Dictionary<MapSlot, string>();
            textures = new List<ShaderResourceView>();
        }

        public bool LoadTextures(Device device)
        {
            try
            {
                foreach (KeyValuePair<MapSlot, string> tex in maps)
                {
                    textures.Add(ShaderResourceView.FromFile(device, tex.Value));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ReleaseTextures()
        {
            textures = new List<ShaderResourceView>();
        }

        public ShaderResourceView[] PackTextures()
        {
            return textures.ToArray<ShaderResourceView>();
        }
    }
}
