using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct PBRLight
    {
        #region constructors
        public static PBRLight Directional(Vector3 dir, Vector3 color, float intensity)
        {
            return new PBRLight
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

        internal static void Encode(PBRLight[] src, Vector4[] p0, Vector4[] p1, Vector4[] p2, Vector4[] p3)
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

        public void ApplyTo(DirectionalLight dlight)
        {
            if (Type == 0)
            {
                dlight.Enabled = true;
                dlight.Direction = this.Direction;
                dlight.DiffuseColor = this.Color;
                dlight.SpecularColor = Vector3.Lerp(this.Color, Vector3.One, 0.75f);
            }
            else
            {
                dlight.Enabled = false;
            }            
        }

        #endregion

        #region nested types

        /// <summary>
        /// To be implemented by effects using <see cref="PBRLight"/> sources.
        /// </summary>
        public interface IEffect
        {
            float Exposure { get; set; }
            void SetLight(int index, PBRLight light);
        }

        #endregion
    };
}
