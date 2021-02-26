using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Represents a 3D model resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lifecycle flow:<br/>
    /// <see cref="ModelContent"/> ➔ <b><see cref="ModelTemplate"/></b> ➔ <see cref="ModelInstance"/>.
    /// </para>
    /// </remarks>
    public class ModelTemplate : BaseContent
    {
        #region lifecycle        

        public ModelTemplate(ModelCollectionContent collection, int index)
        {
            var srcModel = collection._Models[index];

            _Armature = collection._SharedArmatures[srcModel.ArmatureLogicalIndex];
            _Drawables = srcModel.DrawableReferences;

            _ModelBounds = srcModel.ModelBounds;            
        }

        #endregion

        #region data        

        internal readonly ArmatureContent _Armature;        
        internal readonly IReadOnlyList<DrawableContent> _Drawables;

        private readonly Microsoft.Xna.Framework.BoundingSphere _ModelBounds;

        private IMeshCollection _SharedMeshes;
        private Effect[] _SharedEffects;        

        #endregion

        #region properties        

        public Microsoft.Xna.Framework.BoundingSphere ModelBounds => _ModelBounds;

        public IMeshCollection Meshes
        {
            get => _SharedMeshes;
            set
            {
                _SharedMeshes = value;
                _SharedEffects = null;

                // if we change the meshes, we probably need to rebuild _ModelBounds.
            }
        }

        public IReadOnlyCollection<Effect> SharedEffects
        {
            get
            {
                if (_SharedEffects != null) return _SharedEffects;

                var meshIndices = _Drawables.Select(item => item.MeshIndex);
                _SharedEffects = _SharedMeshes.GetSharedEffects(meshIndices);

                return _SharedEffects;
            }
        }

        #endregion

        #region API

        public ModelInstance CreateInstance() => new ModelInstance(this);

        #endregion
    }
}
