using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.Graphics.ModelGraph;

using SharpGLTF.Runtime;

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

           

            
    }
}
