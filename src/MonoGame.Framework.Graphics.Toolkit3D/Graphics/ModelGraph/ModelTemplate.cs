using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;



namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a templatized representation of a <see cref="Schema2.Scene"/> that can be used
    /// to create <see cref="ModelInstance"/>, which can help render a scene on a client application.
    /// </summary>
    public class ModelTemplate
    {
        #region lifecycle        

        public ModelTemplate(string modelName, ArmatureTemplate armature, IDrawableTemplate[] drawables)
        {
            _ModelName = modelName;
            _Armature = armature;
            _DrawableReferences = drawables;            
        }

        #endregion

        #region data

        private readonly String _ModelName;        

        internal readonly ArmatureTemplate _Armature;
        
        private IMeshCollection _Meshes;        

        // this is the collection of "what needs to be rendered", and it binds meshes with armatures
        internal readonly IDrawableTemplate[] _DrawableReferences;

        private Effect[] _SharedEffects;

        #endregion

        #region properties

        public String Name => _ModelName;        

        public BoundingSphere ModelBounds { get; set; }

        public IMeshCollection Meshes
        {
            get => _Meshes;
            set
            {
                _Meshes = value;
                _SharedEffects = null;
            }
        }

        public IReadOnlyCollection<Effect> SharedEffects
        {
            get
            {
                if (_SharedEffects != null) return _SharedEffects;

                var meshIndices = _DrawableReferences.Select(item => item.MeshIndex);
                _SharedEffects = _Meshes.GetSharedEffects(meshIndices);

                return _SharedEffects;
            }
        }

        #endregion

        #region API

        public ModelInstance CreateInstance() => new ModelInstance(this);

        #endregion
    }
}
