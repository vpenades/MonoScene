using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MODELMESH = SharpGLTF.Runtime.RuntimeModelMesh;
using MODELMESHPART = SharpGLTF.Runtime.RuntimeModelMeshPart;

namespace SharpGLTF.Runtime
{
    public sealed class MonoGameModelInstance
    {
        #region lifecycle

        internal MonoGameModelInstance(MonoGameModelTemplate template, SceneInstance instance)
        {
            _Template = template;
            _Controller = instance;
        }

        #endregion

        #region data

        private readonly MonoGameModelTemplate _Template;
        private readonly SceneInstance _Controller;
        private Matrix _WorldMatrix;

        #endregion

        #region properties

        /// <summary>
        /// Gets a reference to the template used to create this <see cref="MonoGameModelInstance"/>.
        /// </summary>
        public MonoGameModelTemplate Template => _Template;

        /// <summary>
        /// Gets a reference to the animation controller of this <see cref="MonoGameModelInstance"/>.
        /// </summary>
        public SceneInstance Controller => _Controller;

        public Matrix WorldMatrix
        {
            get => _WorldMatrix;
            set => _WorldMatrix = value;
        }

        #endregion

        #region API

        /// <summary>
        /// Draws this <see cref="MonoGameModelInstance"/> into the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="world">The world matrix.</param>
        public void Draw(Matrix projection, Matrix view, Matrix world)
        {
            _WorldMatrix = world;
            Draw(projection, view);
        }

        /// <summary>
        /// Draws this <see cref="MonoGameModelInstance"/> into the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>        
        public void Draw(Matrix projection, Matrix view)
        {
            foreach (var d in _Controller.DrawableInstances)
            {
                Draw(_Template._Meshes[d.Template.LogicalMeshIndex], projection, view, _WorldMatrix, d.Transform);
            }
        }

        private void Draw(MODELMESH mesh, Matrix projectionXform, Matrix viewXform, Matrix worldXform, Transforms.IGeometryTransform modelXform)
        {
            if (modelXform is Transforms.SkinnedTransform skinXform)
            {
                var skinTransforms = skinXform.SkinMatrices.Select(item => item.ToXna()).ToArray();

                foreach (var effect in mesh.Effects)
                {
                    UpdateTransforms(effect, projectionXform, viewXform, worldXform, skinTransforms);
                }
            }

            if (modelXform is Transforms.RigidTransform statXform)
            {
                var statTransform = statXform.WorldMatrix.ToXna();

                worldXform = Matrix.Multiply(statTransform, worldXform);

                foreach (var effect in mesh.Effects)
                {
                    UpdateTransforms(effect, projectionXform, viewXform, worldXform);
                }
            }

            mesh.Draw();
        }

        private static void UpdateTransforms(Effect effect, Matrix projectionXform, Matrix viewXform, Matrix worldXform, Matrix[] skinTransforms = null)
        {
            if (effect is IEffectMatrices matrices)
            {
                matrices.Projection = projectionXform;
                matrices.View = viewXform;
                matrices.World = worldXform;
            }

            if (skinTransforms != null)
            {
                if (effect is SkinnedEffect skin)
                {
                    var xposed = skinTransforms.Select(item => Matrix.Transpose(item)).ToArray();

                    skin.SetBoneTransforms(skinTransforms);
                }
                else if (effect is IEffectBones iskin)
                {
                    var xposed = skinTransforms.Select(item => Matrix.Transpose(item)).ToArray();

                    iskin.SetBoneTransforms(skinTransforms, 0, skinTransforms.Length);
                }
            }
            else
            {
                if (effect is IEffectBones iskin)
                {
                    iskin.SetBoneTransforms(null, 0, 0);
                }
            }
        }

        #endregion
    }    
}
