using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class PBREffect : AnimatedEffect, PBRLight.IEffect
    {
        #region lifecycle
        
        public PBREffect(GraphicsDevice device, byte[] effectCode) : base(device, effectCode)
        {
            _NormalMap = new EffectTexture2D.ScalarX(device, this.Parameters, "Normal", 0);
            _EmissiveMap = new EffectTexture2D.ScalarXYZ(device, this.Parameters, "Emissive", 3);
            _OcclusionMap = new EffectTexture2D.ScalarX(device, this.Parameters, "Occlusion", 4);
        }

        #endregion

        #region data        

        private readonly PBRLight[] _Lights = new PBRLight[3];
        private readonly Vector4[] _LightParams0 = new Vector4[3];
        private readonly Vector4[] _LightParams1 = new Vector4[3];
        private readonly Vector4[] _LightParams2 = new Vector4[3];
        private readonly Vector4[] _LightParams3 = new Vector4[3];

        private readonly EffectTexture2D.ScalarX _NormalMap;
        private readonly EffectTexture2D.ScalarXYZ _EmissiveMap;
        private readonly EffectTexture2D.ScalarX _OcclusionMap;

        #endregion

        #region properties - lights
        
        public float Exposure { get; set; } = 1;

        public bool LightingEnabled { get; set; }

        public Vector3 AmbientLightColor { get; set; }

        public PBRLight GetLight(int index) => _Lights[index];

        public void SetLight(int index, PBRLight light) => _Lights[index] = light;

        #endregion

        #region properties - material
        public bool AlphaBlend { get; set; }
        public float AlphaCutoff { get; set; }

        public EffectTexture2D.ScalarX NormalMap => _NormalMap;
        public EffectTexture2D.ScalarXYZ EmissiveMap => _EmissiveMap;
        public EffectTexture2D.ScalarX OcclusionMap => _OcclusionMap;

        #endregion

        #region API

        public void EnableDefaultLighting() { }
        
        protected void ApplyPBR()
        {
            this.ApplyTransforms();            

            PBRLight.Encode(_Lights, _LightParams0, _LightParams1, _LightParams2, _LightParams3);
            Parameters["LightParam0"].SetValue(_LightParams0);
            Parameters["LightParam1"].SetValue(_LightParams1);
            Parameters["LightParam2"].SetValue(_LightParams2);
            Parameters["LightParam3"].SetValue(_LightParams3);

            Parameters["CameraPosition"].SetValue(-View.Translation);
            Parameters["Exposure"].SetValue(this.Exposure);

            Parameters["AlphaTransform"].SetValue(AlphaBlend ? Vector2.UnitX : Vector2.UnitY);
            Parameters["AlphaCutoff"].SetValue(AlphaCutoff);

            Resources.GenerateDotTextures(this.GraphicsDevice);
            _NormalMap.Apply();
            _EmissiveMap.Apply();
            _OcclusionMap.Apply();
        }        

        #endregion
    }

    
}
