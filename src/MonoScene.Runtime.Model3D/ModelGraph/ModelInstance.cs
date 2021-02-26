using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;


namespace MonoScene.Graphics
{
    /// <summary>
    /// Represents the state machine of a specific model instance on screen.    
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lifecycle flow:<br/>
    /// <see cref="Content.ModelContent"/> ➔ <see cref="ModelTemplate"/> ➔ <b><see cref="ModelInstance"/></b>.
    /// </para>
    /// </remarks>
    public class ModelInstance
    {
        #region lifecycle

        internal ModelInstance(ModelTemplate parent)
        {
            _Parent = parent;

            _WorldMatrix = XNAMAT.Identity;

            _ModelArmature = new ArmatureInstance(_Parent._Armature);
            _ModelArmature.SetPoseTransforms();            

            _DrawableInstances = new DrawableInstance[_Parent._Drawables.Count];
            
            for (int i = 0; i < _DrawableInstances.Length; ++i)
            {
                _DrawableInstances[i] = new DrawableInstance(_Parent._Drawables[i]);
            }
        }

        #endregion

        #region data        

        private readonly ModelTemplate _Parent;        

        private XNAMAT _WorldMatrix;

        private readonly ArmatureInstance _ModelArmature;

        private readonly DrawableInstance[] _DrawableInstances;
        
        #endregion

        #region properties

        public string Name => _Parent.Name;

        public Object Tag => _Parent.Tag;

        public ArmatureInstance Armature => _ModelArmature;

        public XNAMAT WorldMatrix
        {
            get => _WorldMatrix;
            set => _WorldMatrix = value;
        }

        public Microsoft.Xna.Framework.BoundingSphere ModelBounds => _Parent.ModelBounds;

        public Microsoft.Xna.Framework.BoundingSphere WorldBounds => _Parent.ModelBounds.Transform(_WorldMatrix);

        /// <summary>
        /// Gets the number of drawable instances.
        /// </summary>
        public int DrawableInstancesCount => _DrawableInstances.Length;

        /// <summary>
        /// Gets the current sequence of drawing commands.
        /// </summary>
        public IEnumerable<DrawableInstance> DrawableInstances
        {
            get
            {
                _UpdateDrawableInstanceTransforms();
                return _DrawableInstances;
            }
        }        

        public IEnumerable<Effect> SharedEffects => _Parent.SharedEffects;

        #endregion

        #region API - Armature

        public int IndexOfNode(string nodeName) { return _ModelArmature.IndexOfNode(nodeName); }

        /// <summary>
        /// Gets the matrix of a given node/bone in Model Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in model space.</returns>
        public XNAMAT GetModelMatrix(int nodeIndex) { return _ModelArmature.LogicalNodes[nodeIndex].ModelMatrix; }

        /// <summary>
        /// Gets the matrix of a given node/bone in World Space.
        /// </summary>
        /// <param name="nodeIndex">The index of the node/bone.</param>
        /// <returns>A matrix in world space.</returns>
        public XNAMAT GetWorldMatrix(int nodeIndex) { return GetModelMatrix(nodeIndex) * _WorldMatrix; }

        #endregion

        #region API - Drawing        

        private void _UpdateDrawableInstanceTransforms()
        {
            foreach (var dwinst in _DrawableInstances) dwinst.Transform.Update(_ModelArmature);
        }

        /// <summary>
        /// Draws this <see cref="MonoGameModelInstance"/> into the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>        
        public void DrawAllParts(XNAMAT projection, XNAMAT view)
        {
            foreach (var e in this.SharedEffects)
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
                var mesh = _Parent.Meshes[d.Content.MeshIndex];
                if (mesh.TranslucidEffects.Count == 0) continue;

                _SetEffectsTransforms(mesh.TranslucidEffects, _WorldMatrix, d.Transform);

                mesh.DrawTranslucid();
            }
        }

        public void DrawOpaqueParts()
        {
            foreach (var d in DrawableInstances)
            {
                var mesh = _Parent.Meshes[d.Content.MeshIndex];
                if (mesh.OpaqueEffects.Count == 0) continue;

                _SetEffectsTransforms(mesh.OpaqueEffects, _WorldMatrix, d.Transform);

                mesh.DrawOpaque();
            }
        }

        /// <summary>
        /// Applies <paramref name="worldXform"/> and <paramref name="meshXform"/> to <paramref name="effects"/>.
        /// </summary>
        /// <param name="effects">The effects to update.</param>
        /// <param name="worldXform">The new world transform.</param>
        /// <param name="meshXform">The new mesh transform containing model and skin matrices.</param>
        private void _SetEffectsTransforms(IReadOnlyCollection<Effect> effects, XNAMAT worldXform, IMeshTransform meshXform)
        {
            if (meshXform.TryGetModelMatrix(out XNAMAT modelXform))
            {
                worldXform = XNAMAT.Multiply(modelXform, worldXform);
            }

            var meshSkinMatrices = meshXform.TryGetSkinMatrices();

            foreach (var effect in effects)
            {
                UpdateWorldTransforms(effect, worldXform, meshSkinMatrices);
            }
        }        

        #endregion

        #region effect utils

        // pre-allocated bone arrays to update the IEffectBones
        private static readonly List<XNAMAT[]> _BoneArrays = new List<XNAMAT[]>();

        // Since SkinnedEffect has such a flexible and GC friendly API,
        // we have to do this to have a reusable bone matrix pool.
        private static XNAMAT[] UseArray(int count)
        {
            while (_BoneArrays.Count <= count) _BoneArrays.Add(null);

            if (_BoneArrays[count] == null) _BoneArrays[count] = new XNAMAT[count];

            return _BoneArrays[count];

        }


        public static void UpdateProjViewTransforms(Effect effect, XNAMAT projectionXform, XNAMAT viewXform)
        {
            if (effect is IEffectMatrices matrices)
            {
                matrices.Projection = projectionXform;
                matrices.View = viewXform;
            }
        }

        public static void UpdateWorldTransforms(Effect effect, XNAMAT worldXform, XNAMAT[] skinTransforms = null)
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

        public static IComparer<ModelInstance> GetDistanceComparer(XNAV3 origin)
        {
            return new _DistanceComparer(origin);
        }

        private struct _DistanceComparer : IComparer<ModelInstance>
        {
            public _DistanceComparer(XNAV3 origin) { _Origin = origin; }

            private readonly XNAV3 _Origin;

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
