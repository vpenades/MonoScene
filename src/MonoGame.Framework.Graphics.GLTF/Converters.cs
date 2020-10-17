using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    static class Converters
    {
        public static Vector2 ToXna(this System.Numerics.Vector2 v) { return new Vector2(v.X, v.Y); }

        public static Vector3 ToXna(this System.Numerics.Vector3 v) { return new Vector3(v.X, v.Y, v.Z); }

        public static Vector4 ToXna(this System.Numerics.Vector4 v) { return new Vector4(v.X, v.Y, v.Z, v.W); }

        public static Quaternion ToXna(this System.Numerics.Quaternion q) { return new Quaternion(q.X, q.Y, q.Z, q.W); }

        public static BoundingSphere ToXna(this (System.Numerics.Vector3 center, float radius) sphere)
        {
            return new BoundingSphere(sphere.center.ToXna(), sphere.radius);
        }

        public static Matrix ToXna(this System.Numerics.Matrix4x4 m)
        {
            return new Matrix
                (m.M11,m.M12,m.M13,m.M14
                ,m.M21,m.M22,m.M23,m.M24
                ,m.M31,m.M32,m.M33,m.M34
                ,m.M41,m.M42,m.M43,m.M44);
        }

        public static (Vector3 U, Vector3 V) ToXna(this System.Numerics.Matrix3x2 m)
        {
            var u = new Vector3(m.M11, m.M21, m.M31);
            var v = new Vector3(m.M12, m.M22, m.M32);

            return (u, v);
        }

        public static ICurveEvaluator<Vector3> ToXna(this SharpGLTF.Animations.ICurveSampler<System.Numerics.Vector3> curve)
        {
            if (curve == null) return null;
            return new _GltfSamplerVector3(curve);
        }

        public static ICurveEvaluator<Quaternion> ToXna(this SharpGLTF.Animations.ICurveSampler<System.Numerics.Quaternion> curve)
        {
            if (curve == null) return null;
            return new _GltfSamplerQuaternion(curve);
        }

        public static AffineTransform ToXna(this SharpGLTF.Transforms.AffineTransform xform)
        {
            return new AffineTransform(xform.Scale.ToXna(), xform.Rotation.ToXna(), xform.Translation.ToXna());
        }

        public static TextureAddressMode ToXna(this SharpGLTF.Schema2.TextureWrapMode mode)
        {
            switch (mode)
            {
                case SharpGLTF.Schema2.TextureWrapMode.CLAMP_TO_EDGE: return TextureAddressMode.Clamp;
                case SharpGLTF.Schema2.TextureWrapMode.MIRRORED_REPEAT: return TextureAddressMode.Mirror;
                default: return TextureAddressMode.Wrap;
            }
        }

        public static TextureFilter ToXna(this (SharpGLTF.Schema2.TextureInterpolationFilter, SharpGLTF.Schema2.TextureMipMapFilter) mode)
        {
            bool isLinear = mode.Item1 != SharpGLTF.Schema2.TextureInterpolationFilter.NEAREST;

            switch (mode.Item2)
            {
                case SharpGLTF.Schema2.TextureMipMapFilter.LINEAR: return isLinear ? TextureFilter.Linear : TextureFilter.Point;
                case SharpGLTF.Schema2.TextureMipMapFilter.NEAREST: return isLinear ? TextureFilter.LinearMipPoint : TextureFilter.Point;
            }

            // TODO: convert all values to closest feature in XNA

            return TextureFilter.Linear; // fallback
        }

        public static MaterialContent ToXna(this GLTFMATERIAL srcMaterial)
        {
            var dstMaterial = new MaterialContent();
            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.DoubleSided = srcMaterial.DoubleSided;

            dstMaterial.AlphaCutoff = srcMaterial.AlphaCutoff;
            switch(srcMaterial.Alpha)
            {
                case SharpGLTF.Schema2.AlphaMode.OPAQUE: dstMaterial.Mode = MaterialBlendMode.Opaque;break;
                case SharpGLTF.Schema2.AlphaMode.MASK: dstMaterial.Mode = MaterialBlendMode.Mask; break;
                case SharpGLTF.Schema2.AlphaMode.BLEND: dstMaterial.Mode = MaterialBlendMode.Blend; break;
            }            

            if (srcMaterial.Unlit) dstMaterial.PreferredShading = "Unlit";
            else if (srcMaterial.FindChannel("SpecularGlossiness") != null) dstMaterial.PreferredShading = "SpecularGlossiness";
            else if (srcMaterial.FindChannel("MetallicRoughness") != null) dstMaterial.PreferredShading = "MetallicRoughness";

            foreach (var srcChannel in srcMaterial.Channels)
            {                 
                var dstChannel = dstMaterial.UseChannel(srcChannel.Key);
                
                dstChannel.Value = ParamToArray(srcChannel);

                if (srcChannel.Texture != null)
                {
                    dstChannel.Texture = srcChannel.Texture.PrimaryImage.Content.Content.ToArray();
                    dstChannel.Sampler = srcChannel.Texture.Sampler.ToXna();
                }
                else
                {
                    dstChannel.Sampler = SamplerStateContent.CreateDefault();
                }

                
                dstChannel.VertexIndexSet = srcChannel.TextureCoordinate;
                dstChannel.Transform = (srcChannel.TextureTransform?.Matrix ?? System.Numerics.Matrix3x2.Identity).ToXna();                
            }

            return dstMaterial;
        }

        private static SamplerStateContent ToXna(this SharpGLTF.Schema2.TextureSampler srcSampler)
        {
            if (srcSampler == null) return SamplerStateContent.CreateDefault();

            return new SamplerStateContent
            {
                AddressU = srcSampler.WrapS.ToXna(),
                AddressV = srcSampler.WrapT.ToXna(),
                AddressW = TextureAddressMode.Wrap,
                Filter = (srcSampler.MagFilter, srcSampler.MinFilter).ToXna()
            };
        }

        public static IReadOnlyList<IMeshDecoder<MaterialContent>> ToXna(this IEnumerable<SharpGLTF.Schema2.Mesh> srcMeshes)
        {
            if (!srcMeshes.Any()) return Array.Empty<IMeshDecoder<MaterialContent>>();

            if (srcMeshes.GroupBy(item => item.LogicalParent).Count() > 1) throw new ArgumentException(nameof(srcMeshes));

            var srcMaterials = srcMeshes.First().LogicalParent.LogicalMaterials;

            var dstMaterials = srcMaterials
                .Select(item => item.ToXna())
                .ToArray();

            var dstMeshes = srcMeshes
                .Select(item => new _MeshDecoder(item.Decode(), dstMaterials))
                .Cast<IMeshDecoder<MaterialContent>>()
                .ToArray();

            return dstMeshes;
        }

        private static float[] ParamToArray(this SharpGLTF.Schema2.MaterialChannel srcChannel)
        {
            var val = srcChannel.Parameter;
            switch(srcChannel.Key)
            {
                case "Normal":
                case "Occlusion":
                    return new float[] { val.X };

                case "MetallicRoughness":
                    return new float[] { val.X, val.Y };

                case "Emissive":
                    return new float[] { val.X, val.Y, val.Z };                

                case "Diffuse":
                case "BaseColor":
                case "SpecularGlossiness":
                    return new float[] { val.X, val.Y, val.Z, val.W };
            }

            throw new NotImplementedException();
        }
    
    }
}
