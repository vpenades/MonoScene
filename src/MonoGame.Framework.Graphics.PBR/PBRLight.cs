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

        /// <summary>
        /// Applies the current settings to a classic Effect's DirectionalLight
        /// </summary>
        /// <param name="dlight">The target light.</param>
        /// <remarks>
        /// Due to the different nature of the way classic effects and PBR effects calculate light, it's
        /// impossible to achieve the same results, so this method uses a "best effort" approach.
        /// </remarks>
        public void ApplyTo(DirectionalLight dlight, float exposure)
        {
            if (Type == 0)
            {
                var sigma2 = 2f - 2f / (1f + (Intensity * exposure) );

                dlight.Enabled = this.Intensity > 0;
                dlight.Direction = this.Direction;
                dlight.DiffuseColor = this.Color * sigma2;
                dlight.SpecularColor = Vector3.Lerp(this.Color * sigma2 * 0.5f, Vector3.One* sigma2, 0.5f);
            }
            else
            {
                dlight.Enabled = false;
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

            void SetPunctualLight(int index, PBRPunctualLight light);
        }

        #endregion
    };
}
