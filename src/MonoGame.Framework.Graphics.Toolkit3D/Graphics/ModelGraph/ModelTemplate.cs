using System;
using System.Collections.Generic;
using System.Text;

using MODELMESH = Microsoft.Xna.Framework.Graphics.RuntimeModelMesh;


namespace Microsoft.Xna.Framework.Graphics
{
    public class ModelTemplate : IDisposable
    {
        #region lifecycle

        public ModelTemplate(MeshCollection meshes, ModelLayerTemplate[] layers, int defaultLayer)
        {            
            _Layers = layers;
            _DefaultLayerIndex = defaultLayer;

            SharedMeshes = meshes;
        }

        public void Dispose()
        {
            _SharedMeshes?.Dispose();
            _SharedMeshes = null;
            _Layers = null;
        }

        #endregion

        #region data

        /// <summary>
        /// Meshes shared by all the <see cref="_Layers"/>.
        /// </summary>
        internal MeshCollection _SharedMeshes;        

        /// <summary>
        /// Layers available in this template
        /// </summary>
        internal ModelLayerTemplate[] _Layers;        

        /// <summary>
        /// Default layer index
        /// </summary>
        private readonly int _DefaultLayerIndex;

        #endregion

        #region properties

        public IReadOnlyList<ModelLayerTemplate> Layers => _Layers;

        public ModelLayerTemplate DefaultLayer => _Layers[_DefaultLayerIndex];

        public MeshCollection SharedMeshes
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
