using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;
using XNAMAT = Microsoft.Xna.Framework.Matrix;

namespace MonoScene
{
    /// <summary>
    /// Represents an affine transform in 3D space, defined by:
    /// - A <see cref="Vector3"/> scale.
    /// - A <see cref="Quaternion"/> rotation.
    /// - A <see cref="Vector3"/> translation.
    /// </summary>
    /// <remarks>
    /// <see cref="AffineTransform"/> cannot represent skewed matrices. This means
    /// that it can be used to represent <see cref="Schema2.Node"/> local transforms,
    /// but since chained transforms can become skewed, a world transform cannot be
    /// represented by a <see cref="AffineTransform"/>.
    /// </remarks>
    /// <see href="https://github.com/vpenades/SharpGLTF/issues/41"/>
    [System.Diagnostics.DebuggerDisplay("AffineTransform 𝐒:{Scale} 𝐑:{Rotation} 𝚻:{Translation}")]
    public struct AffineTransform
    {
        private const float _UnitLengthThresholdVec4 = 0.00769f;

        #region lifecycle

        public static implicit operator AffineTransform(XNAMAT matrix) { return new AffineTransform(matrix); }

        public AffineTransform(XNAMAT matrix)
        {
            if (!matrix.Decompose(out this.Scale, out this.Rotation, out this.Translation))
            {
                throw new ArgumentException("matrix is invalid or skewed.", nameof(matrix));
            }
        }

        public AffineTransform(XNAV3? scale, XNAQUAT? rotation, XNAV3? translation)
        {
            this.Scale = scale ?? XNAV3.One;
            this.Rotation = rotation ?? XNAQUAT.Identity;
            this.Translation = translation ?? XNAV3.Zero;
        }

        public static AffineTransform CreateFromAny(XNAMAT? matrix, XNAV3? scale, XNAQUAT? rotation, XNAV3? translation)
        {
            if (matrix.HasValue)
            {
                return new AffineTransform(matrix.Value);
            }
            else
            {
                return new AffineTransform(scale, rotation, translation);
            }
        }

        #endregion

        #region data        

        /// <summary>
        /// Scale
        /// </summary>
        public XNAV3 Scale;

        /// <summary>
        /// Rotation
        /// </summary>
        public XNAQUAT Rotation;

        /// <summary>
        /// Translation
        /// </summary>
        public XNAV3 Translation;

        #endregion

        #region properties

        public static AffineTransform Identity => new AffineTransform { Rotation = XNAQUAT.Identity, Scale = XNAV3.One, Translation = XNAV3.Zero };

        /// <summary>
        /// Gets the <see cref="Matrix4x4"/> transform of the current <see cref="AffineTransform"/>
        /// </summary>
        public XNAMAT Matrix
        {
            get
            {
                var m = XNAMAT.CreateScale(this.Scale) * XNAMAT.CreateFromQuaternion(Sanitized(this.Rotation));
                m.Translation = this.Translation;
                return m;
            }
        }

        public bool IsIdentity
        {
            get
            {
                if (Scale != XNAV3.One) return false;
                if (Rotation != XNAQUAT.Identity) return false;
                if (Translation != XNAV3.Zero) return false;
                return true;
            }
        }

        #endregion

        #region API

        public static AffineTransform Blend(ReadOnlySpan<AffineTransform> transforms, ReadOnlySpan<float> weights)
        {
            var s = XNAV3.Zero;
            var r = default(XNAQUAT);
            var t = XNAV3.Zero;

            for (int i = 0; i < transforms.Length; ++i)
            {
                var w = weights[i];

                s += transforms[i].Scale * w;
                r += transforms[i].Rotation * w;
                t += transforms[i].Translation * w;
            }

            r = XNAQUAT.Normalize(r);

            return new AffineTransform(s, r, t);
        }

        public static AffineTransform operator *(in AffineTransform a, in AffineTransform b)
        {
            return Multiply(a, b);
        }

        public static AffineTransform Multiply(in AffineTransform a, in AffineTransform b)
        {
            AffineTransform r;

            r.Scale = Vector3Transform(b.Scale * Vector3Transform(a.Scale, a.Rotation), XNAQUAT.Inverse(a.Rotation));

            r.Rotation = XNAQUAT.Multiply(b.Rotation, a.Rotation);

            r.Translation
                = b.Translation
                + Vector3Transform(a.Translation * b.Scale, b.Rotation);

            return r;
        }

        /// <summary>
        /// This method is equivalent to System.Numerics.Vector3.Transform(Vector3 v, Quaternion q)
        /// </summary>
        /// <param name="v">The vector to transform</param>
        /// <param name="q">The transform rotation</param>
        /// <returns>The rotated vector</returns>
        private static XNAV3 Vector3Transform(XNAV3 v, XNAQUAT q)
        {
            // Extract the vector part of the quaternion
            var u = new XNAV3(q.X, q.Y, q.Z);

            // Extract the scalar part of the quaternion
            var s = q.W;

            // Do the math
            return (2.0f * XNAV3.Dot(u, v) * u)
                + (((s * s) - XNAV3.Dot(u, u)) * v)
                + (2.0f * s * XNAV3.Cross(u, v));
        }




        internal static XNAQUAT AsQuaternion(XNAV4 v)
        {
            return new XNAQUAT(v.X, v.Y, v.Z, v.W);
        }

        internal static XNAQUAT Sanitized(XNAQUAT q)
        {
            return IsNormalized(q) ? q : XNAQUAT.Normalize(q);
        }

        internal static Boolean IsNormalized(XNAQUAT rotation)
        {
            // if (!rotation._IsFinite()) return false;

            return Math.Abs(rotation.Length() - 1) <= _UnitLengthThresholdVec4;
        }

        #endregion
    }
}
