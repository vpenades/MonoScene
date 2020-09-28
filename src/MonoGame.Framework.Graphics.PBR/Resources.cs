using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Graphics
{
    static class Resources
    {
        private static GraphicsDevice _Device;

        private static readonly Dictionary<string, Byte[]> _Shaders = new Dictionary<string, byte[]>();

        public static Byte[] GetShaderByteCode(string name)
        {
            if (_Shaders.TryGetValue(name, out Byte[] data)) return data;            

            var assembly = typeof(Resources).Assembly;

            var resources = assembly.GetManifestResourceNames();

            var resName = resources.FirstOrDefault(item => item.EndsWith(name + $".{GetShaderProfile()}.mgfxo"));

            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var mem = new System.IO.MemoryStream())
                {
                    stream.CopyTo(mem);

                    data = mem.ToArray();
                    _Shaders[name] = data;
                    return data;
                }
            }
        }

        internal static string GetShaderProfile()
        {
            var mgAssembly = typeof(Effect).Assembly;
            var shaderType = mgAssembly.GetType("Microsoft.Xna.Framework.Graphics.Shader");
            var profileProperty = shaderType.GetProperty("Profile");
            var value = (int)profileProperty.GetValue(null);
            return value == 1 ? "dx11" : "ogl";
        }

        private static Texture2D whiteDotTexture;
        private static Texture2D blackTransparentDotTexture;
        private static Texture2D aoRoughMetalDefaltDotTexture;

        private static Texture2D bdrf_ibl_ggx;

        public static Texture2D WhiteDotTexture { get { if (whiteDotTexture != null) return whiteDotTexture; else { throw new NullReferenceException("SharpGLTF.Runtime.Generated.WhiteDotTexture()  ... The Generated dot texture was requested but never created. Make sure you have called Initialize(,,) on gltf in monogames load function first."); } } }
        public static Texture2D BlackTransparentDotTexture { get { if (blackTransparentDotTexture != null) return blackTransparentDotTexture; else { throw new NullReferenceException("SharpGLTF.Runtime.Generated.BlackTransparentDotTexture()  ... The Generated dot texture was requested but never created. Make sure you have called Initialize(,,) on gltf in monogames load function first."); } } }
        public static Texture2D AoRoughMetalDefaltDotTexture { get { if (aoRoughMetalDefaltDotTexture != null) return aoRoughMetalDefaltDotTexture; else { throw new NullReferenceException("SharpGLTF.Runtime.Generated.BlackTransparentDotTexture()  ... The Generated dot texture was requested but never created. Make sure you have called Initialize(,,) on gltf in monogames load function first."); } } }

        public static Texture2D IblGGX
        {
            get
            {
                if (bdrf_ibl_ggx != null) return bdrf_ibl_ggx;

                var pixels = BRDFGenerator.Generate(128, val => new Rg32(val));
                bdrf_ibl_ggx = new Texture2D(_Device, 128, 128, false, SurfaceFormat.Rg32);
                bdrf_ibl_ggx.SetData(pixels);

                return bdrf_ibl_ggx;
            }
        }

        public static void GenerateDotTextures(GraphicsDevice device)
        {
            if (_Device != null) return;

            _Device = device;           
            
            whiteDotTexture = new Texture2D(device, 1, 1);
            whiteDotTexture.SetData(new Color[] { Color.White });
                
            blackTransparentDotTexture = new Texture2D(device, 1, 1);
            blackTransparentDotTexture.SetData(new Color[] { Color.Transparent });
                
            aoRoughMetalDefaltDotTexture = new Texture2D(device, 1, 1);
            aoRoughMetalDefaltDotTexture.SetData(new Color[] { new Color(1.0f, 0.97f, 0.03f, 0.0f) });            
        }
    }
}
