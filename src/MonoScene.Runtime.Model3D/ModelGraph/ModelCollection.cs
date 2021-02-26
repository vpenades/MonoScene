using System;
using System.Collections.Generic;
using System.Linq;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Defines a <see cref="ModelCollection"/> super class that has ownership over
    /// <see cref="MeshCollection"/> resources and it's responsible of disposing them.
    /// </summary>
    public class DeviceModelCollection : ModelCollection, IDisposable
    {
        #region lifecycle

        public static DeviceModelCollection CreateFrom(ModelCollectionContent content, Converter<MeshCollectionContent, MeshCollection> meshesConverter)
        {
            var meshes = meshesConverter(content._SharedMeshes);

            var models = content._Models
                .Select((item, idx) => new ModelTemplate(content,idx))
                .ToArray();
            
            return new DeviceModelCollection(meshes, content._SharedArmatures, models, content._DefaultModelIndex);           
        }

        public DeviceModelCollection(MeshCollection meshes, ArmatureContent[] armatures, ModelTemplate[] models, int defaultModelIndex)
            : base(meshes, armatures, models,defaultModelIndex)
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

    /// <summary>
    /// Defines a collection of <see cref="ModelTemplate"/> objects and their shared resources.
    /// </summary>
    public class ModelCollection
    {
        #region lifecycle

        public ModelCollection(IMeshCollection meshes, ArmatureContent[] armatures, ModelTemplate[] models, int defaultModelIndex)
        {
            _SharedArmatures = armatures;
            _DefaultModelIndex = defaultModelIndex;
            _Models = models;

            // must be set after _Models;
            SharedMeshes = meshes;            
        }

        #endregion

        #region data

        /// <summary>
        /// Multiple <see cref="ModelTemplate"/> at <see cref="_Models"/> might share the same meshes.
        /// </summary>
        private IMeshCollection _SharedMeshes;

        /// <summary>
        /// Multiple <see cref="ModelTemplate"/> at <see cref="_Models"/> might share the same <see cref="ArmatureTemplate"/>.
        /// </summary>
        private ArmatureContent[] _SharedArmatures;

        /// <summary>
        /// Models available in this collection.
        /// </summary>
        private ModelTemplate[] _Models;        

        /// <summary>
        /// Default model index
        /// </summary>
        private readonly int _DefaultModelIndex;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the collection of meshes.
        /// </summary>        
        public IMeshCollection SharedMeshes
        {
            get => _SharedMeshes;
            set
            {
                _SharedMeshes = value;
                foreach (var model in _Models) model.Meshes = _SharedMeshes;
            }
        }

        /// <summary>
        /// Gets the list of models.
        /// </summary>
        public IReadOnlyList<ModelTemplate> Models => _Models;

        /// <summary>
        /// Gets the default model.
        /// </summary>
        public ModelTemplate DefaultModel => _Models[_DefaultModelIndex];        

        #endregion

        #region API

        public int IndexOfModel(string modelName) => Array.FindIndex(_Models, item => item.Name == modelName);       

        #endregion
    }
}
