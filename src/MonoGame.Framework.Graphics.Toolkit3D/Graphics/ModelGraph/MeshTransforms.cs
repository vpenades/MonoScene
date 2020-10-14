using System;
using System.Collections.Generic;
using System.Text;

using V3 = Microsoft.Xna.Framework.Vector3;
using V4 = Microsoft.Xna.Framework.Vector4;
using TRANSFORM = Microsoft.Xna.Framework.Matrix;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.VertexInfluences;
using MORPHINFLUENCES = Microsoft.Xna.Framework.Graphics.VertexInfluences;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Interface for a mesh transform object
    /// </summary>
    public interface IMeshTransform
    {
        /// <summary>
        /// Gets a value indicating whether the current <see cref="IMeshTransform"/> will render visible geometry.
        /// </summary>
        /// <remarks>
        /// When this value is false, a runtime should skip rendering any geometry using
        /// this <see cref="IMeshTransform"/> instance, since it will not be visible anyway.
        /// </remarks>
        bool Visible { get; }

        /// <summary>
        /// Gets a value indicating whether the triangles need to be flipped to render correctly.
        /// </summary>
        /// <remarks>
        /// When this value is true, a runtime rendering triangles should inverse the face culling.
        /// </remarks>
        bool FlipFaces { get; }

        
        /// <summary>
        /// Transforms a vertex position from local mesh space to world space.
        /// </summary>
        /// <param name="position">The local position of the vertex.</param>
        /// <param name="positionDeltas">The local position deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A position in world space.</returns>
        V3 TransformPosition(V3 position, IReadOnlyList<V3> positionDeltas, in VERTEXINFLUENCES skinWeights);

        /// <summary>
        /// Transforms a vertex normal from local mesh space to world space.
        /// </summary>
        /// <param name="normal">The local normal of the vertex.</param>
        /// <param name="normalDeltas">The local normal deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A normal in world space.</returns>
        V3 TransformNormal(V3 normal, IReadOnlyList<V3> normalDeltas, in VERTEXINFLUENCES skinWeights);

        /// <summary>
        /// Transforms a vertex tangent from local mesh space to world space.
        /// </summary>
        /// <param name="tangent">The tangent normal of the vertex.</param>
        /// <param name="tangentDeltas">The local tangent deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A tangent in world space.</returns>
        V4 TransformTangent(V4 tangent, IReadOnlyList<V3> tangentDeltas, in VERTEXINFLUENCES skinWeights);

        V4 MorphColors(V4 color, IReadOnlyList<V4> morphTargets);
        
    }

    abstract class MeshMorphTransform
    {
        #region constructor

        protected MeshMorphTransform()
        {
            Update(default, false);
        }

        protected MeshMorphTransform(MORPHINFLUENCES morphWeights, bool useAbsoluteMorphTargets)
        {
            Update(morphWeights, useAbsoluteMorphTargets);
        }

        #endregion

        #region data

        /// <summary>
        /// Represents a sparse collection of weights where:
        /// - Index of value <see cref="COMPLEMENT_INDEX"/> points to the Mesh master positions.
        /// - All other indices point to Mesh MorphTarget[index] positions.
        /// </summary>
        private MORPHINFLUENCES _Weights;

        public const int COMPLEMENT_INDEX = 65536;

        /// <summary>
        /// True if morph targets represent absolute values.
        /// False if morph targets represent values relative to master value.
        /// </summary>
        private bool _AbsoluteMorphTargets;

        #endregion

        #region properties

        /// <summary>
        /// Gets the current morph weights to use for morph target blending. <see cref="COMPLEMENT_INDEX"/> represents the index for the base geometry.
        /// </summary>
        public MORPHINFLUENCES MorphWeights => _Weights;

        /// <summary>
        /// Gets a value indicating whether morph target values are absolute, and not relative to the master value.
        /// </summary>
        public bool AbsoluteMorphTargets => _AbsoluteMorphTargets;

        #endregion

        #region API

        public void Update(MORPHINFLUENCES morphWeights, bool useAbsoluteMorphTargets = false)
        {
            /*
            _AbsoluteMorphTargets = useAbsoluteMorphTargets;

            if (morphWeights.IsWeightless)
            {
                _Weights = MORPHINFLUENCES.Create((COMPLEMENT_INDEX, 1));
                return;
            }

            _Weights = morphWeights.GetNormalizedWithComplement(COMPLEMENT_INDEX);
            */
        }

        protected V3 MorphVectors(V3 value, IReadOnlyList<V3> morphTargets)
        {
            return value;

            /*
            if (morphTargets == null || morphTargets.Count == 0) return value;
            
            if (_Weights.Index0 == COMPLEMENT_INDEX && _Weights.Weight0 == 1) return value;

            var p = V3.Zero;

            if (_AbsoluteMorphTargets)
            {
                foreach (var (index, weight) in _Weights.GetNonZeroWeights())
                {
                    var val = index == COMPLEMENT_INDEX ? value : morphTargets[index];
                    p += val * weight;
                }
            }
            else
            {
                foreach (var (index, weight) in _Weights.GetNonZeroWeights())
                {
                    var val = index == COMPLEMENT_INDEX ? value : value + morphTargets[index];
                    p += val * weight;
                }
            }

            return p;
            */
        }

        protected V4 MorphVectors(V4 value, IReadOnlyList<V4> morphTargets)
        {
            return value;

            /*
            if (morphTargets == null || morphTargets.Count == 0) return value;

            if (_Weights.Index0 == COMPLEMENT_INDEX && _Weights.Weight0 == 1) return value;

            var p = V4.Zero;

            if (_AbsoluteMorphTargets)
            {
                foreach (var pair in _Weights.GetNonZeroWeights())
                {
                    var val = pair.Index == COMPLEMENT_INDEX ? value : morphTargets[pair.Index];
                    p += val * pair.Weight;
                }
            }
            else
            {
                foreach (var pair in _Weights.GetNonZeroWeights())
                {
                    var val = pair.Index == COMPLEMENT_INDEX ? value : value + morphTargets[pair.Index];
                    p += val * pair.Weight;
                }
            }

            return p;
            */
        }

        public V4 MorphColors(V4 color, IReadOnlyList<V4> morphTargets)
        {
            return MorphVectors(color, morphTargets);
        }

        #endregion
    }

    class MeshRigidTransform : MeshMorphTransform, IMeshTransform
    {
        #region constructor

        public MeshRigidTransform()
        {
            Update(TRANSFORM.Identity);
        }

        public MeshRigidTransform(TRANSFORM worldMatrix)
        {
            Update(default, false);
            Update(worldMatrix);
        }

        public MeshRigidTransform(TRANSFORM worldMatrix, MORPHINFLUENCES morphWeights, bool useAbsoluteMorphs)
        {
            Update(morphWeights, useAbsoluteMorphs);
            Update(worldMatrix);
        }

        #endregion

        #region data

        private TRANSFORM _WorldMatrix;
        private Boolean _Visible;
        private Boolean _FlipFaces;

        #endregion

        #region properties

        public Boolean Visible => _Visible;

        public Boolean FlipFaces => _FlipFaces;

        public TRANSFORM WorldMatrix => _WorldMatrix;

        #endregion

        #region API

        public void Update(TRANSFORM worldMatrix)
        {
            _WorldMatrix = worldMatrix;

            // http://m-hikari.com/ija/ija-password-2009/ija-password5-8-2009/hajrizajIJA5-8-2009.pdf

            float determinant3x3 =
                +(worldMatrix.M13 * worldMatrix.M21 * worldMatrix.M32)
                + (worldMatrix.M11 * worldMatrix.M22 * worldMatrix.M33)
                + (worldMatrix.M12 * worldMatrix.M23 * worldMatrix.M31)
                - (worldMatrix.M12 * worldMatrix.M21 * worldMatrix.M33)
                - (worldMatrix.M13 * worldMatrix.M22 * worldMatrix.M31)
                - (worldMatrix.M11 * worldMatrix.M23 * worldMatrix.M32);

            _Visible = Math.Abs(determinant3x3) > float.Epsilon;
            _FlipFaces = determinant3x3 < 0;
        }

        
        public V3 TransformPosition(V3 position, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            position = MorphVectors(position, morphTargets);

            return V3.Transform(position, _WorldMatrix);
        }

        public V3 TransformNormal(V3 normal, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            normal = MorphVectors(normal, morphTargets);

            return V3.Normalize(V3.TransformNormal(normal, _WorldMatrix));
        }

        public V4 TransformTangent(V4 tangent, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            var t = MorphVectors(new V3(tangent.X, tangent.Y, tangent.Z), morphTargets);

            t = V3.Normalize(V3.TransformNormal(t, _WorldMatrix));

            return new V4(t, tangent.W);
        }

        #endregion
    }

    class MeshSkinTransform : MeshMorphTransform, IMeshTransform
    {
        #region constructor

        public MeshSkinTransform() { }

        public MeshSkinTransform(TRANSFORM[] invBindMatrix, TRANSFORM[] currWorldMatrix, MORPHINFLUENCES morphWeights, bool useAbsoluteMorphTargets)
        {
            Update(morphWeights, useAbsoluteMorphTargets);
            Update(invBindMatrix, currWorldMatrix);
        }

        public MeshSkinTransform(int count, Func<int, TRANSFORM> invBindMatrix, Func<int, TRANSFORM> currWorldMatrix, MORPHINFLUENCES morphWeights, bool useAbsoluteMorphTargets)
        {
            Update(morphWeights, useAbsoluteMorphTargets);
            Update(count, invBindMatrix, currWorldMatrix);
        }

        #endregion

        #region data

        private TRANSFORM[] _SkinTransforms;

        #endregion

        #region properties

        public bool Visible => true;
        public bool FlipFaces => false;

        /// <summary>
        /// Gets the collection of the current, final matrices to use for skinning
        /// </summary>
        public IReadOnlyList<TRANSFORM> SkinMatrices => _SkinTransforms;        

        #endregion

        #region API

        public void Update(TRANSFORM[] invBindMatrix, TRANSFORM[] currWorldMatrix)
        {
            // Guard.NotNull(invBindMatrix, nameof(invBindMatrix));
            // Guard.NotNull(currWorldMatrix, nameof(currWorldMatrix));
            // Guard.IsTrue(invBindMatrix.Length == currWorldMatrix.Length, nameof(currWorldMatrix), $"{invBindMatrix} and {currWorldMatrix} length mismatch.");

            if (_SkinTransforms == null || _SkinTransforms.Length != invBindMatrix.Length) _SkinTransforms = new TRANSFORM[invBindMatrix.Length];

            for (int i = 0; i < _SkinTransforms.Length; ++i)
            {
                _SkinTransforms[i] = invBindMatrix[i] * currWorldMatrix[i];
            }
        }

        public void Update(int count, Func<int, TRANSFORM> invBindMatrix, Func<int, TRANSFORM> currWorldMatrix)
        {
            // Guard.NotNull(invBindMatrix, nameof(invBindMatrix));
            // Guard.NotNull(currWorldMatrix, nameof(currWorldMatrix));

            if (_SkinTransforms == null || _SkinTransforms.Length != count) _SkinTransforms = new TRANSFORM[count];

            for (int i = 0; i < _SkinTransforms.Length; ++i)
            {
                _SkinTransforms[i] = invBindMatrix(i) * currWorldMatrix(i);
            }
        }        
        
        public V3 TransformPosition(V3 localPosition, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            localPosition = MorphVectors(localPosition, morphTargets);

            var worldPosition = V3.Zero;

            var wnrm = 1.0f / skinWeights.WeightSum;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldPosition += V3.Transform(localPosition, _SkinTransforms[jidx]) * jweight * wnrm;
            }

            return worldPosition;
        }

        public V3 TransformNormal(V3 localNormal, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            localNormal = MorphVectors(localNormal, morphTargets);

            var worldNormal = V3.Zero;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldNormal += V3.TransformNormal(localNormal, _SkinTransforms[jidx]) * jweight;
            }

            return V3.Normalize(localNormal);
        }

        public V4 TransformTangent(V4 localTangent, IReadOnlyList<V3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            var localTangentV = MorphVectors(new V3(localTangent.X, localTangent.Y, localTangent.Z), morphTargets);

            var worldTangent = V3.Zero;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldTangent += V3.TransformNormal(localTangentV, _SkinTransforms[jidx]) * jweight;
            }

            worldTangent = V3.Normalize(worldTangent);

            return new V4(worldTangent, localTangent.W);
        }

        #endregion        
    }
}
