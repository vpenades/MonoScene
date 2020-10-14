using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
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

        public TextureFilter Filter { get; set; }
        public TextureAddressMode AddressU { get; set; }
        public TextureAddressMode AddressV { get; set; }
        public TextureAddressMode AddressW { get; set; }

        public SamplerState CreateState()
        {
            return new SamplerState()
            {
                Filter = this.Filter,
                AddressU = this.AddressU,
                AddressV = this.AddressV,
                AddressW = this.AddressW
            };
        }

        public SamplerState TryGetPredefinedSampler()
        {
            if (AddressU != AddressV) return null;

            if (Filter == TextureFilter.Point)
            {
                if (AddressU == TextureAddressMode.Clamp) return SamplerState.PointClamp;
                if (AddressU == TextureAddressMode.Wrap) return SamplerState.PointWrap;
            }

            if (Filter == TextureFilter.Linear)
            {
                if (AddressU == TextureAddressMode.Clamp) return SamplerState.LinearClamp;
                if (AddressU == TextureAddressMode.Wrap) return SamplerState.LinearWrap;
            }

            if (Filter == TextureFilter.Anisotropic)
            {
                if (AddressU == TextureAddressMode.Clamp) return SamplerState.AnisotropicClamp;
                if (AddressU == TextureAddressMode.Wrap) return SamplerState.AnisotropicWrap;
            }

            return null;
        }
    }
}
