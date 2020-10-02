using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents the state machine of a specific model instance on screen.    
    /// </summary>
    /// <remarks>
    /// For each <see cref="ModelTemplate"/> you can create
    /// multiple <see cref="ModelInstance"/> objects.
    /// </remarks>
    public class ModelInstance
    {
        #region lifecycle

        internal ModelInstance(ModelTemplate parent)
        {
            _Parent = parent;
            
            _Armature = new ArmatureInstance(_Parent._Armature);
            _Armature.SetPoseTransforms();

            _WorldMatrix = Matrix.Identity;            

            _DrawableTemplates = _Parent._DrawableReferences;
            _DrawableTransforms = new IMeshTransform[_DrawableTemplates.Length];

            for (int i = 0; i < _DrawableTransforms.Length; ++i)
            {
                _DrawableTransforms[i] = _DrawableTemplates[i].CreateGeometryTransform();
            }            
        }

        #endregion

        #region data        

        private readonly ModelTemplate _Parent;

        private IMeshCollection _Meshes => _Parent.Meshes;

        private readonly ArmatureInstance _Armature;

        private readonly IDrawableTemplate[] _DrawableTemplates;
        private readonly IMeshTransform[] _DrawableTransforms;

        private Matrix _WorldMatrix;        

        #endregion

        #region properties

        /// <summary>
        /// Gets a reference to the template used to create this <see cref="MonoGameModelInstance"/>.
        /// </summary>
        public ModelTemplate Template => _Parent;

        public ArmatureInstance Armature => _Armature;

        public Matrix WorldMatrix
        {
            get => _WorldMatrix;
            set => _WorldMatrix = value;
        }

        public BoundingSphere ModelBounds => _Parent.ModelBounds;

        public BoundingSphere WorldBounds => _Parent.ModelBounds.Transform(_WorldMatrix);

        /// <summary>
        /// Gets the number of drawable instances.
        /// </summary>
        public int DrawableInstancesCount => _DrawableTransforms.Length;

        /// <summary>
        /// Gets the current sequence of drawing commands.
        /// </summary>
        public IEnumerable<DrawableInstance> DrawableInstances
        {
            get
            {
                for (int i = 0; i < _DrawableTemplates.Length; ++i)
                {
                    yield return GetDrawableInstance(i);
                }
            }
        }

        #endregion

        #region API - Armature

        public int IndexOfNode(string nodeName) { return _Armature.IndexOfNode(nodeName); }

        /// <summary>
        /// Gets the matrix of a given node/bone in Model Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in model space.</returns>
        public Matrix GetModelMatrix(int nodeIndex) { return _Armature.LogicalNodes[nodeIndex].ModelMatrix; }

        /// <summary>
        /// Gets the matrix of a given node/bone in World Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in world space.</returns>
        public Matrix GetWorldMatrix(int nodeIndex) { return GetModelMatrix(nodeIndex) * _WorldMatrix; }

        #endregion

        #region API - Drawing

        /// <summary>
        /// Gets a <see cref="DrawableInstance"/> object, where:
        /// - Name is the name of this drawable instance. Originally, it was the name of <see cref="Schema2.Node"/>.
        /// - MeshIndex is the logical Index of a <see cref="Schema2.Mesh"/> in <see cref="Schema2.ModelRoot.LogicalMeshes"/>.
        /// - Transform is an <see cref="IMeshTransform"/> that can be used to transform the <see cref="Schema2.Mesh"/> into world space.
        /// </summary>
        /// <param name="index">The index of the drawable reference, from 0 to <see cref="DrawableInstancesCount"/></param>
        /// <returns><see cref="DrawableInstance"/> object.</returns>
        public DrawableInstance GetDrawableInstance(int index)
        {
            var dref = _DrawableTemplates[index];

            dref.UpdateGeometryTransform(_DrawableTransforms[index], _Armature);

            return new DrawableInstance(dref, _DrawableTransforms[index]);
        }

        /// <summary>
        /// Draws this <see cref="MonoGameModelInstance"/> into the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>        
        public void DrawAllParts(Matrix projection, Matrix view)
        {
            foreach (var e in this.Template.SharedEffects)
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
            foreach (var d in DrawableInstances)
            {
                var mesh = _Meshes[d.Template.MeshIndex];
                if (mesh.TranslucidEffects.Count == 0) continue;

                SetEffectsTransforms(mesh.TranslucidEffects, _WorldMatrix, d.Transform);

                mesh.DrawTranslucid();
            }
        }

        public void DrawOpaqueParts()
        {
            foreach (var d in DrawableInstances)
            {
                var mesh = _Meshes[d.Template.MeshIndex];
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
        private void SetEffectsTransforms(IReadOnlyCollection<Effect> effects, Matrix worldXform, IMeshTransform meshXform)
        {
            if (meshXform is MeshSkinTransform skinnedXform)
            {
                // skinned transforms don't have a single "local transform" instead, they deform the mesh using multiple meshes.

                var skinTransforms = UseArray(skinnedXform.SkinMatrices.Count);

                for (int i = 0; i < skinTransforms.Length; ++i)
                {
                    skinTransforms[i] = skinnedXform.SkinMatrices[i];
                }

                foreach (var effect in effects)
                {
                    UpdateWorldTransforms(effect, worldXform, skinTransforms);
                }
            }

            if (meshXform is MeshRigidTransform rigidXform)
            {
                var statTransform = rigidXform.WorldMatrix;

                worldXform = Matrix.Multiply(statTransform, worldXform);

                foreach (var effect in effects)
                {
                    UpdateWorldTransforms(effect, worldXform);
                }
            }
        }

        #endregion

        #region effect utils

        // pre-allocated bone arrays to update the IEffectBones
        private static readonly List<Matrix[]> _BoneArrays = new List<Matrix[]>();

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

        public static void UpdateWorldTransforms(Effect effect, Matrix worldXform, Matrix[] skinTransforms = null)
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

        public static IComparer<ModelInstance> GetDistanceComparer(Vector3 origin)
        {
            return new _DistanceComparer(origin);
        }

        private struct _DistanceComparer : IComparer<ModelInstance>
        {
            public _DistanceComparer(Vector3 origin) { _Origin = origin; }

            private readonly Vector3 _Origin;

            public int Compare(ModelInstance x, ModelInstance y)
            {
                var xDist = (x.WorldMatrix.Translation - _Origin).LengthSquared();
                var yDist = (y.WorldMatrix.Translation - _Origin).LengthSquared();

                return xDist.CompareTo(yDist);
            }
        }

        #endregion
    }
}
