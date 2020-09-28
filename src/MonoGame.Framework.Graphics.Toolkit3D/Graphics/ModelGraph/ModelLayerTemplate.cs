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
    /// to create <see cref="ModelLayerInstance"/>, which can help render a scene on a client application.
    /// </summary>
    public class ModelLayerTemplate
    {
        #region lifecycle        

        public ModelLayerTemplate(string layerName, ArmatureTemplate armature, IDrawableTemplate[] drawables)
        {
            _LayerName = layerName;
            _Armature = armature;
            _DrawableReferences = drawables;            
        }

        #endregion

        #region data

        private readonly String _LayerName;        

        internal readonly ArmatureTemplate _Armature;
        
        private MeshCollection _Meshes;        

        // this is the collection of "what needs to be rendered", and it binds meshes with armatures
        internal readonly IDrawableTemplate[] _DrawableReferences;

        private Effect[] _SharedEffects;

        #endregion

        #region properties

        public String Name => _LayerName;        

        public BoundingSphere ModelBounds { get; set; }

        public MeshCollection Meshes
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

                // gather all effects used by all the meshes used by all the drawable calls in this layer.
                _SharedEffects = _DrawableReferences                    
                    .Select(item => _Meshes[item.MeshIndex])
                    .SelectMany(item => item.OpaqueEffects.Concat(item.TranslucidEffects))
                    .Distinct()
                    .ToArray();

                return _SharedEffects;
            }
        }

        #endregion

        #region API

        public ModelLayerInstance CreateInstance() => new ModelLayerInstance(this);

        #endregion
    }
}
