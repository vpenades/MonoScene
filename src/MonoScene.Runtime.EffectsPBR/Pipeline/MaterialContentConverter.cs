using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

using TEXTURELOOKUP = System.Converter<int, Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Helper object used to convert a <see cref="MaterialContent"/> to a <see cref="PBREffect"/>
    /// </summary>
    readonly struct MaterialContentConverter
    {
        #region constructor
        public MaterialContentConverter(MaterialContent content, TEXTURELOOKUP texture)
        {
            _MaterialSource = content;
            _TextureSource = texture;
        }

        #endregion

        #region data

        private readonly MaterialContent _MaterialSource;
        private readonly TEXTURELOOKUP _TextureSource;

        #endregion

        #region API

        public string PreferredShading => _MaterialSource.PreferredShading;

        public void CopyToEffect(EffectTexture2D.Scalar1 dst, string name, float defval)
        {
            dst.Texture = UseChannelTexture(name);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar2 dst, string name, XNAV2 defval)
        {
            dst.Texture = UseChannelTexture(name);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar3 dst, string name, XNAV3 defval)
        {
            dst.Texture = UseChannelTexture(name);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        public void CopyToEffect(EffectTexture2D.Scalar4 dst, string name, XNAV4 defval)
        {
            dst.Texture = UseChannelTexture(name);
            dst.Sampler = UseChannelSampler(name);
            dst.Scale = GetScaler(name, defval);
            dst.SetIndex = GetTextureSet(name);
            dst.Transform = GetTransform(name);
        }

        private (XNAV3 u, XNAV3 v) GetTransform(string name)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? (XNAV3.UnitX, XNAV3.UnitY) : channel.Transform;
        }

        private int GetTextureSet(string name)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? 0 : channel.VertexIndexSet;
        }

        private float GetScaler(string name, float defval)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? defval : channel.Value[0];
        }

        private XNAV2 GetScaler(string name, XNAV2 defval)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? defval : new XNAV2(channel.Value[0], channel.Value[1]);
        }

        private XNAV3 GetScaler(string name, XNAV3 defval)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? defval : new XNAV3(channel.Value[0], channel.Value[1], channel.Value[2]);
        }

        private XNAV4 GetScaler(string name, XNAV4 defval)
        {
            var channel = _MaterialSource.FindChannel(name);
            return channel == null ? defval : new XNAV4(channel.Value[0], channel.Value[1], channel.Value[2], channel.Value[3]);
        }

        private Texture2D UseChannelTexture(string channelName)
        {
            var channel = _MaterialSource.FindChannel(channelName);
            if (channel == null) return null;
            return _TextureSource(channel.TextureIndex);
        }

        private SamplerState UseChannelSampler(string name)
        {
            var channel = _MaterialSource.FindChannel(name);
            if (channel == null) return null;

            var state = channel.Sampler.TryGetPredefinedSampler();
            if (state == null)
            {
                state = channel.Sampler.CreateState();
                //TODO: record for disposing
            }

            return state;
        }

        #endregion
    }
}
