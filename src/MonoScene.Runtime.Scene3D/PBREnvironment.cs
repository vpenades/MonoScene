using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines all athmospheric and lighting properties for a given scene setup.
    /// </summary>
    public class PBREnvironment
    {
        #region constants

        public static PBREnvironment CreateDefault()
        {
            var env = new PBREnvironment();
            env.SetDirectLight(0, (20, 30), Color.White, 3.5f);
            env.SetDirectLight(1, (-70, 60), Color.DeepSkyBlue, 1.5f);
            env.SetDirectLight(2, (50, -50), Color.OrangeRed, 0.5f);
            return env;
        }

        #endregion

        #region data

        private float _Exposure = 2.5f;
        private Vector3 _AmbientLight = Vector3.Zero;
        private readonly List<PBRPunctualLight> _PunctualLights = new List<PBRPunctualLight>();

        #endregion

        #region API

        public void SetExposure(float exposure) { _Exposure = exposure; }

        public void SetAmbientLight(Vector3 color) { _AmbientLight = color; }

        public void SetDirectLight(int idx, (int direction, int elevation) degrees, Color color, float intensity)
        {
            _SetPunctualLight(idx, PBRPunctualLight.Directional(degrees, color.ToVector3(), intensity));
        }

        public void SetDirectLight(int idx, Vector3 direction, Color color, float intensity)
        {
            _SetPunctualLight(idx, PBRPunctualLight.Directional(direction, color.ToVector3(), intensity));
        }

        private void _SetPunctualLight(int idx, PBRPunctualLight l)
        {
            while (_PunctualLights.Count <= idx) _PunctualLights.Add(default);
            _PunctualLights[idx] = l;
        }

        public void ApplyTo(Effect effect)
        {
            if (effect is IEffectFog fog) { fog.FogEnabled = false; }

            PBRPunctualLight.ApplyLights(effect, _Exposure, _AmbientLight, _PunctualLights);
        }

        #endregion
    }
}
