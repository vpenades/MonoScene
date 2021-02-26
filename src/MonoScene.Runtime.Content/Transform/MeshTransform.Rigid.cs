using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics
{
    sealed class _MeshRigidTransform : _MeshMorphTransform, IMeshTransform
    {
        #region constructor

        public _MeshRigidTransform(Content.RigidDrawableContent owner)
        {
            _Owner = owner;
            Update(XNAMAT.Identity);

            // Update(default, false);
        }

        #endregion

        #region data

        private readonly Content.RigidDrawableContent _Owner;

        private XNAMAT _WorldMatrix;
        private Boolean _Visible;
        private Boolean _FlipFaces;

        #endregion

        #region properties

        public Boolean Visible => _Visible;

        public Boolean FlipFaces => _FlipFaces;

        public XNAMAT WorldMatrix => _WorldMatrix;

        #endregion

        #region API

        public void Update(IArmatureTransform armature)
        {
            // TODO: Update morph targets

            Update(armature.GetModelMatrix(_Owner._NodeIndex));
        }

        public bool TryGetModelMatrix(out XNAMAT modelMatrix)
        {
            modelMatrix = _WorldMatrix;
            return true;
        }

        public XNAMAT[] TryGetSkinMatrices()
        {
            return null;
        }



        public void Update(XNAMAT worldMatrix)
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


        public XNAV3 TransformPosition(XNAV3 position, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            position = MorphVectors(position, morphTargets);

            return XNAV3.Transform(position, _WorldMatrix);
        }

        public XNAV3 TransformNormal(XNAV3 normal, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            normal = MorphVectors(normal, morphTargets);

            return XNAV3.Normalize(XNAV3.TransformNormal(normal, _WorldMatrix));
        }

        public XNAV4 TransformTangent(XNAV4 tangent, IReadOnlyList<XNAV3> morphTargets, in VERTEXINFLUENCES skinWeights)
        {
            var t = MorphVectors(new XNAV3(tangent.X, tangent.Y, tangent.Z), morphTargets);

            t = XNAV3.Normalize(XNAV3.TransformNormal(t, _WorldMatrix));

            return new XNAV4(t, tangent.W);
        }

        

        #endregion
    }
}
