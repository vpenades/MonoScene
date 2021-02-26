using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics
{
    sealed class _MeshSkinTransform : _MeshMorphTransform, IMeshTransform
    {
        #region lifecycle
        public _MeshSkinTransform(Content.SkinnedDrawableContent owner)
        {
            _Owner = owner;
        }

        #endregion

        #region data

        private readonly Content.SkinnedDrawableContent _Owner;

        private XNAMAT[] _SkinTransforms;

        #endregion

        #region properties

        public bool Visible => true;
        public bool FlipFaces => false;

        /// <summary>
        /// Gets the collection of the current, final matrices to use for skinning
        /// </summary>
        public IReadOnlyList<XNAMAT> SkinMatrices => _SkinTransforms;

        #endregion

        #region API

        public void Update(IArmatureTransform armature)
        {
            // TODO: Update morph targets

            Update(_Owner._JointsNodeIndices.Length
                , idx => _Owner._JointsBindMatrices[idx]
                , idx => armature.GetModelMatrix(_Owner._JointsNodeIndices[idx]));
        }

        public bool TryGetModelMatrix(out XNAMAT modelMatrix)
        {
            modelMatrix = XNAMAT.Identity;
            return false;
        }

        public XNAMAT[] TryGetSkinMatrices()
        {
            return _SkinTransforms;
        }

        public void Update(XNAMAT[] invBindMatrix, XNAMAT[] currWorldMatrix)
        {
            // Guard.NotNull(invBindMatrix, nameof(invBindMatrix));
            // Guard.NotNull(currWorldMatrix, nameof(currWorldMatrix));
            // Guard.IsTrue(invBindMatrix.Length == currWorldMatrix.Length, nameof(currWorldMatrix), $"{invBindMatrix} and {currWorldMatrix} length mismatch.");

            if (_SkinTransforms == null || _SkinTransforms.Length != invBindMatrix.Length) _SkinTransforms = new XNAMAT[invBindMatrix.Length];

            for (int i = 0; i < _SkinTransforms.Length; ++i)
            {
                _SkinTransforms[i] = invBindMatrix[i] * currWorldMatrix[i];
            }
        }

        public void Update(int count, Func<int, XNAMAT> invBindMatrix, Func<int, XNAMAT> currWorldMatrix)
        {
            // Guard.NotNull(invBindMatrix, nameof(invBindMatrix));
            // Guard.NotNull(currWorldMatrix, nameof(currWorldMatrix));

            if (_SkinTransforms == null || _SkinTransforms.Length != count) _SkinTransforms = new XNAMAT[count];

            for (int i = 0; i < _SkinTransforms.Length; ++i)
            {
                _SkinTransforms[i] = invBindMatrix(i) * currWorldMatrix(i);
            }
        }

        public XNAV3 TransformPosition(XNAV3 localPosition, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            localPosition = MorphVectors(localPosition, morphTargets);

            var worldPosition = XNAV3.Zero;

            var wnrm = 1.0f / skinWeights.WeightSum;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldPosition += XNAV3.Transform(localPosition, _SkinTransforms[jidx]) * jweight * wnrm;
            }

            return worldPosition;
        }

        public XNAV3 TransformNormal(XNAV3 localNormal, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            localNormal = MorphVectors(localNormal, morphTargets);

            var worldNormal = XNAV3.Zero;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldNormal += XNAV3.TransformNormal(localNormal, _SkinTransforms[jidx]) * jweight;
            }

            return XNAV3.Normalize(localNormal);
        }

        public XNAV4 TransformTangent(XNAV4 localTangent, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            var localTangentV = MorphVectors(new XNAV3(localTangent.X, localTangent.Y, localTangent.Z), morphTargets);

            var worldTangent = XNAV3.Zero;

            foreach (var (jidx, jweight) in skinWeights.GetIndexedWeights())
            {
                worldTangent += XNAV3.TransformNormal(localTangentV, _SkinTransforms[jidx]) * jweight;
            }

            worldTangent = XNAV3.Normalize(worldTangent);

            return new XNAV4(worldTangent, localTangent.W);
        }

        #endregion        
    }
}
