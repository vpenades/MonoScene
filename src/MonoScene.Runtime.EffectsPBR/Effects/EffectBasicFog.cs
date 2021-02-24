using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    sealed class EffectBasicFog : IEffectFog
    {
        #region lifecycle

        internal EffectBasicFog(GraphicsDevice gd, EffectParameterCollection parameters)
        {
            _Device = gd;

            _FogRange = parameters["FogRange"];
            _FogColor = parameters["FogColor"];
        }

        #endregion

        #region data

        private GraphicsDevice _Device;

        internal EffectParameter _FogRange;
        internal EffectParameter _FogColor;

        #endregion

        #region properties

        public bool FogEnabled { get; set; }
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        public Vector3 FogColor { get; set; }

        #endregion

        #region API        

        public void Apply()
        {
            if (!FogEnabled)
            {
                _FogRange.SetValue(Vector2.Zero);
                return;
            }

            var scale = FogStart < FogEnd ? 1.0f / (FogEnd - FogStart) : 9999999999f;

            _FogRange.SetValue(new Vector2(FogStart, scale));
            _FogColor.SetValue(FogColor);
        }

        #endregion
    }
}
