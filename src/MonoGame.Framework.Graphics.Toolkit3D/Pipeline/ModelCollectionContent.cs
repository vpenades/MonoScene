using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    /// <summary>
    /// Represents a data only representation of a collection of models with shared data resources.
    /// </summary>
    public class ModelCollectionContent
    {
        #region constructor

        public ModelCollectionContent(MeshCollectionContent meshes, ArmatureTemplate[] armatures, ModelTemplate[] models, int defaultModelIndex)
        {
            _SharedMeshes = meshes;
            _SharedArmatures = armatures;
            _Models = models;
            _DefaultModelIndex = defaultModelIndex;            
        }

        #endregion

        #region data

        /// <summary>
        /// Multiple <see cref="ModelTemplate"/> at <see cref="_Models"/> might share the same meshes.
        /// </summary>
        private MeshCollectionContent _SharedMeshes;

        /// <summary>
        /// Multiple <see cref="ModelTemplate"/> at <see cref="_Models"/> might share the same <see cref="ArmatureTemplate"/>.
        /// </summary>
        private ArmatureTemplate[] _SharedArmatures;

        /// <summary>
        /// Models available in this collection.
        /// </summary>
        private ModelTemplate[] _Models;

        /// <summary>
        /// Default model index
        /// </summary>
        private readonly int _DefaultModelIndex;

        #endregion

        #region API

        public DeviceModelCollection ToDeviceModelCollection(MeshFactory factory)
        {
            var meshes = factory.CreateMeshCollection(_SharedMeshes);
            return new DeviceModelCollection(meshes, _SharedArmatures, _Models, _DefaultModelIndex);
        }

        #endregion
    }
}
