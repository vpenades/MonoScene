using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct PBRLight
    {
        public interface IEffect
        {
            float Exposure { get; set; }
            void SetLight(int index, PBRLight light);
        }

        public static PBRLight Directional(Vector3 dir, float range, Vector3 color, float intensity)
        {
            return new PBRLight
            {
                Direction = dir,
                Range = range,
                Color = color,
                Intensity = intensity,
                Type = 0
            };
        }

        public Vector3 Direction;
        public float Range;

        public Vector3 Color;
        public float Intensity;

        public Vector3 Position;
        public float InnerConeCos;

        public float OuterConeCos;
        public int Type;

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
    };
}
