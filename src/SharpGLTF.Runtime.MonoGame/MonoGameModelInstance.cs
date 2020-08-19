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

        internal MonoGameModelInstance(MonoGameModelTemplate template,int sceneIndex, SceneInstance instance)
        {
            _Template = template;
            _SceneIndex = sceneIndex;
            _Controller = instance;
        }

        #endregion

        #region data

        private readonly MonoGameModelTemplate _Template;
        private readonly int _SceneIndex;
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
            // first we draw all the opaque meshes
            foreach (var d in _Controller.DrawableInstances)
            {
                var mesh = _Template._Meshes[d.Template.LogicalMeshIndex];
                if (mesh.OpaqueEffects.Count == 0) continue;

                SetEffectsTransforms(mesh.OpaqueEffects, projection, view, _WorldMatrix, d.Transform);

                mesh.DrawOpaque();
            }

            // next, we draw all the translucid meshes
            foreach (var d in _Controller.DrawableInstances)
            {
                var mesh = _Template._Meshes[d.Template.LogicalMeshIndex];
                if (mesh.TranslucidEffects.Count == 0) continue;

                SetEffectsTransforms(mesh.TranslucidEffects, projection, view, _WorldMatrix, d.Transform);

                mesh.DrawTranslucid();
            }
        }

        /// <summary>
        /// Sets the effects transforms.
        /// </summary>
        /// <param name="effects">The target effects</param>
        /// <param name="projectionXform">The current projection matrix</param>
        /// <param name="viewXform">The current view matrix</param>
        /// <param name="worldXform">The current world matrix</param>
        /// <param name="meshXform">The mesh local transform provided by the runtime</param>
        private void SetEffectsTransforms(IReadOnlyCollection<Effect> effects, Matrix projectionXform, Matrix viewXform, Matrix worldXform, Transforms.IGeometryTransform meshXform)
        {            
            if (meshXform is Transforms.SkinnedTransform skinnedXform)
            {
                // skinned transforms don't have a single "local transform" instead, they deform the mesh using multiple meshes.

                var skinTransforms = skinnedXform.SkinMatrices.Select(item => item.ToXna()).ToArray();

                foreach (var effect in effects)
                {
                    UpdateTransforms(effect, projectionXform, viewXform, worldXform, skinTransforms);
                }
            }
            
            if (meshXform is Transforms.RigidTransform rigidXform)
            {
                var statTransform = rigidXform.WorldMatrix.ToXna();

                worldXform = Matrix.Multiply(statTransform, worldXform);

                foreach (var effect in effects)
                {
                    UpdateTransforms(effect, projectionXform, viewXform, worldXform);
                }
            }            
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
                    skin.SetBoneTransforms(skinTransforms);
                }
                else if (effect is IEffectBones iskin)
                {
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
