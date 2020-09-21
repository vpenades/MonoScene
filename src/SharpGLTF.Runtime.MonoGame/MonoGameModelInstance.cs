using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SharpGLTF.Runtime
{
    /// <summary>
    /// Represents the state machine of a specific model instance on screen.    
    /// </summary>
    /// <remarks>
    /// For each <see cref="MonoGameModelTemplate"/> you can have
    /// multiple <see cref="MonoGameModelInstance"/> objects.
    /// </remarks>
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

        // pre-allocated bone arrays to update the IEffectBones
        private static readonly List<Matrix[]> _BoneArrays = new List<Matrix[]>();

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

        public int IndexOfNode(string nodeName)
        {
            for(int i=0; i < _Controller.LogicalNodes.Count; ++i)
            {
                if (_Controller.LogicalNodes[i].Name == nodeName) return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the matrix of a given node/bone in Model Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in model space.</returns>
        public Matrix GetModelMatrix(int nodeIndex)
        {
            return _Controller.LogicalNodes[nodeIndex].WorldMatrix.ToXna();
        }

        /// <summary>
        /// Gets the matrix of a given node/bone in World Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in world space.</returns>
        public Matrix GetWorldMatrix(int nodeIndex)
        {
            return _Controller.LogicalNodes[nodeIndex].WorldMatrix.ToXna() * _WorldMatrix;
        }

        /// <summary>
        /// Draws this <see cref="MonoGameModelInstance"/> into the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>        
        public void Draw(Matrix projection, Matrix view)
        {
            foreach (var e in this.Template.Effects)
            {                
                UpdateProjViewTransforms(e, projection, view);
            }

            // first we draw all the opaque meshes
            DrawOpaqueParts();

            // next, we draw all the translucid meshes
            DrawTranslucidParts();
        }

        public void DrawTranslucidParts()
        {
            foreach (var d in _Controller.DrawableInstances)
            {
                var mesh = _Template._Meshes[d.Template.LogicalMeshIndex];
                if (mesh.TranslucidEffects.Count == 0) continue;

                SetEffectsTransforms(mesh.TranslucidEffects, _WorldMatrix, d.Transform);

                mesh.DrawTranslucid();
            }
        }

        public void DrawOpaqueParts()
        {
            foreach (var d in _Controller.DrawableInstances)
            {
                var mesh = _Template._Meshes[d.Template.LogicalMeshIndex];
                if (mesh.OpaqueEffects.Count == 0) continue;

                SetEffectsTransforms(mesh.OpaqueEffects, _WorldMatrix, d.Transform);

                mesh.DrawOpaque();
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
        private void SetEffectsTransforms(IReadOnlyCollection<Effect> effects, Matrix worldXform, Transforms.IGeometryTransform meshXform)
        {            
            if (meshXform is Transforms.SkinnedTransform skinnedXform)
            {
                // skinned transforms don't have a single "local transform" instead, they deform the mesh using multiple meshes.

                var skinTransforms = UseArray(skinnedXform.SkinMatrices.Count);

                for(int i=0; i < skinTransforms.Length; ++i)
                {
                    skinTransforms[i] = skinnedXform.SkinMatrices[i].ToXna();
                }                

                foreach (var effect in effects)
                {                    
                    UpdateWorldTransforms(effect, worldXform, skinTransforms);
                }                
            }
            
            if (meshXform is Transforms.RigidTransform rigidXform)
            {
                var statTransform = rigidXform.WorldMatrix.ToXna();

                worldXform = Matrix.Multiply(statTransform, worldXform);

                foreach (var effect in effects)
                {                    
                    UpdateWorldTransforms(effect, worldXform);
                }
            }            
        }

        // Since SkinnedEffect has such a flexible and GC friendly API,
        // we have to do this to have a reusable bone matrix pool.
        private static Matrix[] UseArray(int count)
        {
            while (_BoneArrays.Count <= count) _BoneArrays.Add(null);

            if (_BoneArrays[count] == null) _BoneArrays[count] = new Matrix[count];

            return _BoneArrays[count];

        }


        public static void UpdateProjViewTransforms(Effect effect, Matrix projectionXform, Matrix viewXform)
        {
            if (effect is IEffectMatrices matrices)
            {
                matrices.Projection = projectionXform;
                matrices.View = viewXform;
            }
        }

        private static void UpdateWorldTransforms(Effect effect, Matrix worldXform, Matrix[] skinTransforms = null)
        {
            if (effect is IEffectMatrices matrices)
            {
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
                    iskin.SetBoneTransforms(skinTransforms);
                }
            }
            else
            {
                if (effect is IEffectBones iskin)
                {
                    iskin.SetBoneTransforms(null);
                }
            }
        }

        #endregion

        #region nested types

        public static IComparer<MonoGameModelInstance> GetDistanceComparer(Vector3 origin)
        {
            return new _DistanceComparer(origin);
        }

        private struct _DistanceComparer : IComparer<MonoGameModelInstance>
        {
            public _DistanceComparer(Vector3 origin) { _Origin = origin; }

            private readonly Vector3 _Origin;

            public int Compare(MonoGameModelInstance x, MonoGameModelInstance y)
            {
                var xDist = (x.WorldMatrix.Translation - _Origin).LengthSquared();
                var yDist = (y.WorldMatrix.Translation - _Origin).LengthSquared();

                return xDist.CompareTo(yDist);
            }
        }

        #endregion
    }
}
