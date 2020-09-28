using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class TextureFactory<TTexture>
    {
        #region lifecycle

        public TextureFactory(GraphicsDevice device)
        {
            _Device = device;            
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;
        // private readonly GraphicsResourceTracker _Disposables;

        private readonly Dictionary<TTexture, Texture2D> _Textures = new Dictionary<TTexture, Texture2D>();

        #endregion

        #region API

        protected GraphicsDevice Device => _Device;

        public Texture2D UseTexture(TTexture image, string name = null)
        {
            if (_Device == null) throw new InvalidOperationException();            

            if (_Textures.TryGetValue(image, out Texture2D tex)) return tex;

            tex = ConvertTexture(image);

            tex.Name = name;

            _Textures[image] = tex;

            return tex;
        }

        protected abstract Texture2D ConvertTexture(TTexture image);

        public Texture2D UseWhiteImage()
        {
            throw new NotImplementedException();

            /*
            const string solidWhitePNg = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAFHpUWHRUaXRsZQAACJkrz8gsSQUABoACIippo0oAAAAoelRYdEF1dGhvcgAACJkLy0xOzStJVQhIzUtMSS1WcCzKTc1Lzy8BAG89CQyAoFAQAAAAGklEQVQoz2P8//8/AymAiYFEMKphVMPQ0QAAVW0DHZ8uFaIAAAAASUVORK5CYII=";
            var toBytes = Convert.FromBase64String(solidWhitePNg);
            return UseTexture(new ArraySegment<byte>(toBytes), "_InternalSolidWhite");
            */
        }

        public SamplerState UseSampler(TextureAddressMode u, TextureAddressMode v)
        {
            if (u == v)
            {
                if (u == TextureAddressMode.Wrap) return SamplerState.LinearWrap;
                if (u == TextureAddressMode.Clamp) return SamplerState.LinearClamp;
            }

            var dstSampler = new SamplerState();
            // _TextureSamplers[gltfSampler] = dstSampler;
            

            dstSampler.AddressU = u;
            dstSampler.AddressV = v;

            // ToDo: we also need to set magnification and minification filters.

            return dstSampler;
        }

        #endregion        
    }

    public sealed class SolidColorTextureFactory : TextureFactory<Color>
    {
        public SolidColorTextureFactory(GraphicsDevice device, int width = 4, int height = 4) : base(device)
        {
            _Width = width;
            _Height = height;
        }

        private readonly int _Width;
        private readonly int _Height;

        protected override Texture2D ConvertTexture(Color color)
        {
            var data = new Color[_Width * _Height];
            data.AsSpan().Fill(color);

            var tex = new Texture2D(Device, _Width, _Height);
            tex.SetData(data);

            return tex;
        }
    }
}
