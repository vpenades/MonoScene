using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

using MonoScene.Graphics.Content;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

using XYZ = System.Numerics.Vector3;

namespace MonoScene.Graphics.Pipeline
{
    static class Converters
    {
        public static (Vector3 U, Vector3 V) ToXna(this System.Numerics.Matrix3x2 m)
        {
            var u = new Vector3(m.M11, m.M21, m.M31);
            var v = new Vector3(m.M12, m.M22, m.M32);

            return (u, v);
        }

        public static ICurveEvaluator<Vector3> ToXna(this SharpGLTF.Animations.ICurveSampler<XYZ> curve)
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
            if (xform.IsMatrix) xform = xform.GetDecomposed();
            return new AffineTransform(xform.Scale, xform.Rotation, xform.Translation);
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

        public static IReadOnlyList<IMeshDecoder<int>> ToXnaDecoders(this IEnumerable<SharpGLTF.Schema2.Mesh> srcMeshes, GLTFMaterialsFactory materialFactory, Converter<SharpGLTF.Schema2.ExtraProperties, Object> tagConverter)
        {
            if (!srcMeshes.Any()) return Array.Empty<IMeshDecoder<int>>();

            if (srcMeshes.GroupBy(item => item.LogicalParent).Count() > 1) throw new ArgumentException(nameof(srcMeshes));            

            var dstMeshes = srcMeshes
                .Select(item => new _MeshDecoder(item.Decode(), materialFactory, tagConverter?.Invoke(item)))
                .Cast<IMeshDecoder<int>>()
                .ToArray();

            return dstMeshes;
        }        
    
    }
}
