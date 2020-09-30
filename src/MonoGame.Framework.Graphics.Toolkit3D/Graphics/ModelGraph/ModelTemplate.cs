using System;
using System.Collections.Generic;
using System.Text;

using MODELMESH = Microsoft.Xna.Framework.Graphics.RuntimeModelMesh;


namespace Microsoft.Xna.Framework.Graphics
{
    public class ModelTemplateContent : ModelTemplate, IDisposable
    {
        #region lifecycle

        public ModelTemplateContent(MeshCollection meshes, ModelLayerTemplate[] layers, int defaultLayer) :base(meshes,layers,defaultLayer)
        {
            SharedMeshes = meshes;
        }

        public void Dispose()
        {
            _SharedMeshes?.Dispose();
            _SharedMeshes = null;
        }

        #endregion

        #region data

        /// <summary>
        /// Meshes shared by all the <see cref="_Layers"/>.
        /// </summary>
        private MeshCollection _SharedMeshes;

        #endregion
    }


    public class ModelTemplate
    {
        #region lifecycle

        public ModelTemplate(IMeshCollection meshes, ModelLayerTemplate[] layers, int defaultLayer)
        {            
            _Layers = layers;
            _DefaultLayerIndex = defaultLayer;

            SharedMeshes = meshes;
        }        

        #endregion

        #region data

        /// <summary>
        /// Meshes shared by all the <see cref="_Layers"/>.
        /// </summary>
        private IMeshCollection _SharedMeshes;        

        /// <summary>
        /// Layers available in this template
        /// </summary>
        private ModelLayerTemplate[] _Layers;        

        /// <summary>
        /// Default layer index
        /// </summary>
        private readonly int _DefaultLayerIndex;

        #endregion

        #region properties

        public IReadOnlyList<ModelLayerTemplate> Layers => _Layers;

        public ModelLayerTemplate DefaultLayer => _Layers[_DefaultLayerIndex];

        public IMeshCollection SharedMeshes
        {
            get => _SharedMeshes;
            set
            {
                _SharedMeshes = value;
                foreach (var layer in _Layers) layer.Meshes = _SharedMeshes;
            }
        }

        #endregion

        #region API

        public int IndexOfLayer(string layerName) => Array.FindIndex(_Layers, item => item.Name == layerName);       

        #endregion
    }
}
