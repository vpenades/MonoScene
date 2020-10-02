using System;
using System.Collections.Generic;
using System.Text;

using MODELMESH = Microsoft.Xna.Framework.Graphics.RuntimeModelMesh;


namespace Microsoft.Xna.Framework.Graphics
{
    public class ModelCollectionContent : ModelCollection, IDisposable
    {
        #region lifecycle

        public ModelCollectionContent(MeshCollection meshes, ModelTemplate[] models, int defaultModelIndex) :base(meshes,models,defaultModelIndex)
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
        /// Meshes shared by all the <see cref="ModelCollection.Models"/>.
        /// </summary>
        private MeshCollection _SharedMeshes;

        #endregion
    }


    public class ModelCollection
    {
        #region lifecycle

        public ModelCollection(IMeshCollection meshes, ModelTemplate[] models, int defaultModelIndex)
        {            
            _Models = models;
            _DefaultModelIndex = defaultModelIndex;

            SharedMeshes = meshes;
        }        

        #endregion

        #region data

        /// <summary>
        /// Meshes shared by all the <see cref="_Models"/>.
        /// </summary>
        private IMeshCollection _SharedMeshes;        

        /// <summary>
        /// Models available in this template
        /// </summary>
        private ModelTemplate[] _Models;        

        /// <summary>
        /// Default model index
        /// </summary>
        private readonly int _DefaultModelIndex;

        #endregion

        #region properties

        public IMeshCollection SharedMeshes
        {
            get => _SharedMeshes;
            set
            {
                _SharedMeshes = value;
                foreach (var model in _Models) model.Meshes = _SharedMeshes;
            }
        }

        public IReadOnlyList<ModelTemplate> Models => _Models;

        public ModelTemplate DefaultModel => _Models[_DefaultModelIndex];        

        #endregion

        #region API

        public int IndexOfModel(string modelName) => Array.FindIndex(_Models, item => item.Name == modelName);       

        #endregion
    }
}
