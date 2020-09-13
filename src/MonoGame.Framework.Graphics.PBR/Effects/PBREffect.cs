using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class PBREffect : AnimatedEffect, PBRPunctualLight.IEffect , IEffectFog
    {
        #region lifecycle
        
        public PBREffect(GraphicsDevice device, byte[] effectCode) : base(device, effectCode)
        {
            _NormalMap = new EffectTexture2D.Scalar1(device, this.Parameters, "Normal", 0);
            _EmissiveMap = new EffectTexture2D.Scalar3(device, this.Parameters, "Emissive", 3);
            _OcclusionMap = new EffectTexture2D.Scalar1(device, this.Parameters, "Occlusion", 4);

            _Fog = new EffectBasicFog(device, this.Parameters);
        }

        #endregion

        #region data        

        private readonly PBRPunctualLight[] _Lights = new PBRPunctualLight[3];
        private readonly Vector4[] _LightParams0 = new Vector4[3];
        private readonly Vector4[] _LightParams1 = new Vector4[3];
        private readonly Vector4[] _LightParams2 = new Vector4[3];
        private readonly Vector4[] _LightParams3 = new Vector4[3];

        private readonly EffectTexture2D.Scalar1 _NormalMap;
        private readonly EffectTexture2D.Scalar3 _EmissiveMap;
        private readonly EffectTexture2D.Scalar1 _OcclusionMap;

        private readonly EffectBasicFog _Fog;

        #endregion

        #region properties - lights

        /// <summary>
        /// Gets or sets the lighting exposure, which directly affect the apparent brightness of the render.
        /// </summary>
        /// <remarks>
        /// PBR lighting is calculated in LINEAR RGB space, which exceeds the limits of screen sRGB space,
        /// In order to convert from linear to screen space, we need to "Tone Map" the linear RGB color.
        /// </remarks>
        public float Exposure { get; set; } = 1;
        
        public Vector3 AmbientLightColor { get; set; }        

        public int MaxPunctualLights => 3;

        public void SetPunctualLight(int index, PBRPunctualLight light) => _Lights[index] = light;

        #endregion

        #region properties - material
        
        /// <summary>
        /// Gets or sets the mode indicating whether geometry normals must be flipped in the pixel shader.        
        /// </summary>
        /// <remarks>
        /// <see cref="GeometryNormalMode.Default"/> is the preferred mode for standard geometry.
        /// <see cref="GeometryNormalMode.Reverse"/> will flip the normals, producing an "inside out" effect.
        /// <see cref="GeometryNormalMode.DoubleSided"/> will flip the normals only when the normal is
        ///   pointing in opposite direction to the camera. Notice that this mode, altough it can be a general
        ///   solution for all kinds of geometry, it produces artifacts one normals are close to perpendicular
        ///   to the camera. This mode is recommended to be used along with double sided, thin geometris like
        ///   tree leafs, glass, etc.
        /// </remarks>
        public GeometryNormalMode NormalMode { get; set; }        

        public EffectTexture2D.Scalar1 NormalMap => _NormalMap;
        public EffectTexture2D.Scalar3 EmissiveMap => _EmissiveMap;
        public EffectTexture2D.Scalar1 OcclusionMap => _OcclusionMap;

        #endregion

        #region properties - fog
        public Vector3 FogColor { get => _Fog.FogColor; set => _Fog.FogColor = value; }
        public bool FogEnabled { get => _Fog.FogEnabled; set => _Fog.FogEnabled = value; }
        public float FogEnd { get => _Fog.FogEnd; set => _Fog.FogEnd = value; }
        public float FogStart { get => _Fog.FogStart; set => _Fog.FogStart = value; }
        #endregion

        #region API

        protected override void OnApply()
        {
            base.OnApply();

            switch (this.NormalMode)
            {
                case GeometryNormalMode.Default: Parameters["NormalsMode"].SetValue(2f); break;
                case GeometryNormalMode.Reverse: Parameters["NormalsMode"].SetValue(-2f); break;
                case GeometryNormalMode.DoubleSided: Parameters["NormalsMode"].SetValue(0f); break;
            }

            Parameters["Exposure"].SetValue(this.Exposure);
            Parameters["AmbientLight"].SetValue(this.AmbientLightColor);

            PBRPunctualLight.Encode(_Lights, _LightParams0, _LightParams1, _LightParams2, _LightParams3);
            Parameters["LightParam0"].SetValue(_LightParams0);
            Parameters["LightParam1"].SetValue(_LightParams1);
            Parameters["LightParam2"].SetValue(_LightParams2);
            Parameters["LightParam3"].SetValue(_LightParams3);

            _Fog.Apply();

            Resources.GenerateDotTextures(this.GraphicsDevice); // temporary hack

            _NormalMap.Apply();
            _EmissiveMap.Apply();
            _OcclusionMap.Apply();
        }    

        #endregion
    }

    public enum GeometryNormalMode
    {
        /// <summary>
        /// Default normal mode.
        /// </summary>
        Default,

        /// <summary>
        /// Normal direction is reversed.
        /// </summary>
        Reverse,

        /// <summary>
        /// Normal direction is reversed only when back face is visible.
        /// </summary>
        DoubleSided,
    }

    
}
