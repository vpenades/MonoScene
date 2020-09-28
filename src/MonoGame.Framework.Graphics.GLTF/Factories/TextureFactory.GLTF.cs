using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;


using GLTFIMAGE = SharpGLTF.Memory.MemoryImage;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    class GLTFTextureFactory : TextureFactory<GLTFIMAGE>
    {
        public GLTFTextureFactory(GraphicsDevice device)
            : base(device) { }

        protected override Texture2D ConvertTexture(GLTFIMAGE image)
        {
            if (Device == null) throw new InvalidOperationException();

            if (!image.IsValid) return null;

            using (var m = image.Open())
            {
                var tex = Texture2D.FromStream(Device, m);               

                // tex.Name = name;                

                return tex;
            }
        }
    }
}
