using System;
using System.Collections.Generic;
using System.Text;

using TEXFILTER = Microsoft.Xna.Framework.Graphics.TextureFilter;
using TEXADDRESS = Microsoft.Xna.Framework.Graphics.TextureAddressMode;
using TEXSAMPLER = Microsoft.Xna.Framework.Graphics.SamplerState;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Structure used to create <see cref="SamplerState"/> objects.
    /// </summary>
    public struct SamplerStateContent
    {
        public static SamplerStateContent CreateDefault()
        {
            return new SamplerStateContent
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = TextureFilter.Linear
            };
        }

        public TEXFILTER Filter { get; set; }
        public TEXADDRESS AddressU { get; set; }
        public TEXADDRESS AddressV { get; set; }
        public TEXADDRESS AddressW { get; set; }

        public TEXSAMPLER CreateState()
        {
            return new TEXSAMPLER()
            {
                Filter = this.Filter,
                AddressU = this.AddressU,
                AddressV = this.AddressV,
                AddressW = this.AddressW
            };
        }

        public TEXSAMPLER TryGetPredefinedSampler()
        {
            if (AddressU != AddressV) return null;

            if (Filter == TEXFILTER.Point)
            {
                if (AddressU == TEXADDRESS.Clamp) return TEXSAMPLER.PointClamp;
                if (AddressU == TEXADDRESS.Wrap) return TEXSAMPLER.PointWrap;
            }

            if (Filter == TEXFILTER.Linear)
            {
                if (AddressU == TEXADDRESS.Clamp) return TEXSAMPLER.LinearClamp;
                if (AddressU == TEXADDRESS.Wrap) return TEXSAMPLER.LinearWrap;
            }

            if (Filter == TEXFILTER.Anisotropic)
            {
                if (AddressU == TEXADDRESS.Clamp) return TEXSAMPLER.AnisotropicClamp;
                if (AddressU == TEXADDRESS.Wrap) return TEXSAMPLER.AnisotropicWrap;
            }

            return null;
        }
    }
}
