using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;
using MORPHINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics
{
    abstract class _MeshMorphTransform
    {
        #region lifecycle

        protected _MeshMorphTransform()
        {
            Update(default, false);
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

        protected XNAV3 MorphVectors(XNAV3 value, IReadOnlyList<XNAV3> morphTargets)
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

        protected XNAV4 MorphVectors(XNAV4 value, IReadOnlyList<XNAV4> morphTargets)
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

        public XNAV4 MorphColors(XNAV4 color, IReadOnlyList<XNAV4> morphTargets)
        {
            return MorphVectors(color, morphTargets);
        }

        #endregion
    }
}
