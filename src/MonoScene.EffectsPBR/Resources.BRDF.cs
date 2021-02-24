using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Graphics
{
    static class BRDFGenerator
    {
        // based on code from:
        // https://github.com/HectorMF/BRDFGenerator
        // https://github.com/HectorMF/BRDFGenerator/blob/master/BRDFGenerator/BRDFGenerator.cpp
        // explained here: 
        // https://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
        // https://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_slides.pdf

        private const float PI = (float)Math.PI;

        private static float RadicalInverse_VdC(uint bits)
        {
            bits = (bits << 16) | (bits >> 16);
            bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
            bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
            bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
            bits = ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
            return (float)(bits * 2.3283064365386963e-10);
        }

        private static Vector2 Hammersley(uint i, uint N)
        {
            return new Vector2((float)i / (float)N, RadicalInverse_VdC(i));
        }

        private static Vector3 ImportanceSampleGGX(Vector2 Xi, float roughness, Vector3 N)
        {
            float a = roughness * roughness;

            float phi = 2.0f * PI * Xi.X;
            float cosTheta = (float)Math.Sqrt((1.0 - Xi.Y) / (1.0 + (a * a - 1.0) * Xi.Y));
            float sinTheta = (float)Math.Sqrt(1.0 - cosTheta * cosTheta);

            // from spherical coordinates to cartesian coordinates
            Vector3 H;
            H.X = (float)Math.Cos(phi) * sinTheta;
            H.Y = (float)Math.Sin(phi) * sinTheta;
            H.Z = cosTheta;

            // from tangent-space vector to world-space sample vector
            var up = Math.Abs(N.Z) < 0.999f ? Vector3.UnitZ : Vector3.UnitX;
            var tangent = Vector3.Normalize(Vector3.Cross(up, N));
            var bitangent = Vector3.Cross(N, tangent);

            var sampleVec = tangent * H.X + bitangent * H.Y + N * H.Z;
            return Vector3.Normalize(sampleVec);
        }

        private static float GeometrySchlickGGX(float NdotV, float roughness)
        {
            float a = roughness;
            float k = (a * a) / 2.0f;

            float nom = NdotV;
            float denom = NdotV * (1.0f - k) + k;

            return nom / denom;
        }

        private static float GeometrySmith(float roughness, float NoV, float NoL)
        {
            float ggx2 = GeometrySchlickGGX(NoV, roughness);
            float ggx1 = GeometrySchlickGGX(NoL, roughness);

            return ggx1 * ggx2;
        }

        private static Vector2 IntegrateBRDF(float NdotV, float roughness, uint samples)
        {
            var V = new Vector3((float)Math.Sqrt(1.0 - NdotV * NdotV), 0, NdotV);

            float A = 0;
            float B = 0;

            var N = Vector3.UnitZ;

            for (uint i = 0; i < samples; ++i)
            {
                var Xi = Hammersley(i, samples);
                var H = ImportanceSampleGGX(Xi, roughness, N);
                var L = Vector3.Normalize(2.0f * Vector3.Dot(V, H) * H - V);

                float NoL = Math.Max(L.Z, 0.0f);
                float NoH = Math.Max(H.Z, 0.0f);
                float VoH = Math.Max(Vector3.Dot(V, H), 0.0f);
                float NoV = Math.Max(Vector3.Dot(N, V), 0.0f);

                if (NoL > 0.0)
                {
                    float G = GeometrySmith(roughness, NoV, NoL);

                    float G_Vis = (G * VoH) / (NoH * NoV);
                    float Fc = (float)Math.Pow(1.0 - VoH, 5.0);

                    A += (1.0f - Fc) * G_Vis;
                    B += Fc * G_Vis;
                }
            }

            return new Vector2(A / (float)samples, B / (float)samples);
        }

        /// <summary>
        /// Generates the pixels of the BRDF LUT texture.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format</typeparam>
        /// <param name="size">The width and height size of the square texture.</param>
        /// <param name="pixelConverter">a converter from Vector2 to your final pixel format</param>
        /// <returns>A size x size array with the final pixels</returns>
        /// <remarks>
        /// The end result must look exactly as the image of <see href="https://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_slides.pdf"/> on page 13.
        /// </remarks>
        public static TPixel[] Generate<TPixel>(int size, Func<Vector2,TPixel> pixelConverter)
        {
            uint samples = 1024;

            var tex = new TPixel[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float NoV = (y + 0.5f) * (1.0f / size);
                    float roughness = (x + 0.5f) * (1.0f / size);

                    var value = IntegrateBRDF(NoV, roughness, samples);

                    tex[y * size + (size - x - 1)] = pixelConverter(value);
                }
            }

            return tex;
        }
    }
}
