using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a light source with parameters required for PBR punctual lighting.
    /// </summary>
    public struct PBRPunctualLight
    {
        #region constructors

        public static PBRPunctualLight Directional((int direction, int elevation) degrees, Vector3 color, float intensity)
        {
            float yaw = (float)(degrees.direction * Math.PI) / 180.0f;
            float pitch = (float)(degrees.elevation * Math.PI) / 180.0f;
            var xform = Matrix.CreateFromYawPitchRoll(yaw + 3.141592f, pitch, 0);
            var dir = Vector3.Transform(Vector3.UnitZ, xform);

            return Directional(dir, color, intensity);
        }

        public static PBRPunctualLight Directional(Vector3 dir, Vector3 color, float intensity)
        {
            return new PBRPunctualLight
            {
                Direction = dir,                
                Color = color,
                Intensity = intensity,
                Range = 0,
                Type = 0
            };
        }

        #endregion

        #region data

        /// <summary>
        /// Light Type
        /// 0 - Directional
        /// 1 - Point
        /// 2 - Spot
        /// </summary>
        public int Type;

        /// <summary>
        /// Light direction (Only Directional and Spot lights)
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Range (Spot and Point lights)
        /// </summary>
        public float Range;

        public Vector3 Color;

        public float Intensity;

        /// <summary>
        /// Light source position (Spot and Point lights)
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Spot light inner angle
        /// </summary>
        public float InnerConeCos;

        /// <summary>
        /// Spot light outer angle
        /// </summary>
        public float OuterConeCos;        

        #endregion

        #region API
        
        /// <summary>
        /// Applies the current set of PBR lights to the given <see cref="Effect"/>.
        /// </summary>
        /// <param name="effect">The target <see cref="Effect"/>.</param>
        /// <param name="exposure">Exposure factor.</param>
        /// <param name="alight">Ambient light color.</param>
        /// <param name="plights">Punctual lights collection.</param>
        /// <remarks>
        /// This methos works with effects using <see cref="IEffectLights"/> and <see cref="IEffect"/>.
        /// </remarks>
        public static void ApplyLights(Effect effect, float exposure, Vector3 alight, IReadOnlyList<PBRPunctualLight> plights)
        {
            var pbrLights = GetEffectInterface(effect);
            if (pbrLights == null) return;
            
            pbrLights.Exposure = exposure;
            pbrLights.AmbientLightColor = alight;

            for (int i = 0; i < pbrLights.MaxPunctualLights; ++i)
            {
                var l = i < plights.Count ? plights[i] : default;
                pbrLights.SetPunctualLight(i, l);
            }            
        }
        
        /// <summary>
        /// Retrieves a <see cref="IEffect"/> interface fro the given <paramref name="effect"/>.
        /// </summary>
        /// <param name="effect">The <see cref="Effect"/> we want to update.</param>
        /// <returns>An PBR lights interface</returns>
        /// <remarks>
        /// If the effect supports the interface, it's returned immediately.
        /// If the effect supports the classic <see cref="IEffectLights"/>
        /// it returns a wrapper that will do a best effort to convert the lights.
        /// </remarks>
        public static IEffect GetEffectInterface(Effect effect)
        {
            if (effect is IEffect pbrLightsEffect) return pbrLightsEffect;
            if (effect is IEffectLights classicLightsEffect) return new ClassicLightsAdapter(classicLightsEffect);
            return null;
        }

        internal static void Encode(PBRPunctualLight[] src, Vector4[] p0, Vector4[] p1, Vector4[] p2, Vector4[] p3)
        {
            for (int i = 0; i < src.Length; ++i)
            {
                var l = src[i];
                p0[i] = new Vector4(l.Direction, l.Range);
                p1[i] = new Vector4(l.Color, l.Intensity);
                p2[i] = new Vector4(l.Position, l.InnerConeCos);
                p3[i] = new Vector4(l.OuterConeCos, l.Type, 0, 0);
            }
        }

        #endregion

        #region nested types

        /// <summary>
        /// To be implemented by effects using <see cref="PBRPunctualLight"/> sources.
        /// </summary>
        public interface IEffect
        {
            float Exposure { get; set; }

            Vector3 AmbientLightColor { get; set; }

            int MaxPunctualLights { get; }

            void SetPunctualLight(int index, PBRPunctualLight light);
        }

        /// <summary>
        /// this structure wraps in-built monogame effects supporting classic <see cref="IEffectLights"/>
        /// interface and adapts PBR lights to classic lights so we can have some level of compatibility.
        /// </summary>
        /// <remarks>
        /// Due to the different nature of the way classic effects and PBR effects calculate light, it's
        /// impossible to achieve the same results, so this structure uses a "best effort" approach.
        /// </remarks>
        private struct ClassicLightsAdapter : IEffect
        {
            public ClassicLightsAdapter(IEffectLights effect)
            {
                _Effect = effect;
                _Exposure = 2.5f;

                _Effect.LightingEnabled = true;
            }

            private IEffectLights _Effect;
            private float _Exposure;

            public float Exposure
            {
                get => _Exposure;
                set => _Exposure = value;
            }

            private float _GetExposureSigma()
            {
                return 4f - 4f / (1f + _Exposure);
            }

            public Vector3 AmbientLightColor
            {
                get => _Effect.AmbientLightColor / _GetExposureSigma();
                set => _Effect.AmbientLightColor = value * _GetExposureSigma();
            }

            public int MaxPunctualLights => 3;

            public void SetPunctualLight(int index, PBRPunctualLight light)
            {
                switch(index)
                {
                    case 0: ApplyTo(light, _Effect.DirectionalLight0, _Exposure); break;
                    case 1: ApplyTo(light, _Effect.DirectionalLight1, _Exposure); break;
                    case 2: ApplyTo(light, _Effect.DirectionalLight2, _Exposure); break;
                }
            }

            private static void ApplyTo(PBRPunctualLight src, DirectionalLight dst, float exposure)
            {
                if (src.Type == 0)
                {
                    var sigma2 = 2f - 2f / (1f + (src.Intensity * exposure));

                    dst.Enabled = src.Intensity > 0;
                    dst.Direction = src.Direction;
                    dst.DiffuseColor = src.Color * sigma2;
                    dst.SpecularColor = Vector3.Lerp(src.Color * sigma2 * 0.5f, Vector3.One * sigma2, 0.5f);
                }
                else
                {
                    dst.Enabled = false;
                }
            }
        }

        #endregion
    };
}
