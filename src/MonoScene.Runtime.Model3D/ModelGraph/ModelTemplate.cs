﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Defines a templatized representation of a <see cref="Schema2.Scene"/> that can be used
    /// to create <see cref="ModelInstance"/>, which can help render a scene on a client application.
    /// </summary>
    public class ModelTemplate : BaseContent
    {
        #region lifecycle        

        public ModelTemplate(string modelName, ArmatureContent armature, IDrawableTemplate[] drawables)
            : base(modelName)
        {            
            _Armature = armature;
            _DrawableReferences = drawables;            
        }

        #endregion

        #region data        

        internal readonly ArmatureContent _Armature;

        // this is the collection of "what needs to be rendered", and it binds meshes with armatures
        internal readonly IDrawableTemplate[] _DrawableReferences;

        private IMeshCollection _SharedMeshes;
        private Effect[] _SharedEffects;

        #endregion

        #region properties        

        public Microsoft.Xna.Framework.BoundingSphere ModelBounds { get; set; }

        public IMeshCollection Meshes
        {
            get => _SharedMeshes;
            set
            {
                _SharedMeshes = value;
                _SharedEffects = null;
            }
        }

        public IReadOnlyCollection<Effect> SharedEffects
        {
            get
            {
                if (_SharedEffects != null) return _SharedEffects;

                var meshIndices = _DrawableReferences.Select(item => item.MeshIndex);
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
