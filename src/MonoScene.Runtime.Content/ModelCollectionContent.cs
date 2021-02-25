using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Represents a data only representation of a collection of models with shared data resources.
    /// </summary>
    public class ModelCollectionContent
    {
        #region constructor

        public ModelCollectionContent(MeshCollectionContent meshes, ArmatureContent[] armatures, ModelContent[] models, int defaultModelIndex)
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
        public MeshCollectionContent _SharedMeshes;

        /// <summary>
        /// Multiple <see cref="ModelTemplate"/> at <see cref="_Models"/> might share the same <see cref="ArmatureTemplate"/>.
        /// </summary>
        public ArmatureContent[] _SharedArmatures;

        /// <summary>
        /// Models available in this collection.
        /// </summary>
        public ModelContent[] _Models;

        /// <summary>
        /// Default model index
        /// </summary>
        public readonly int _DefaultModelIndex;

        #endregion        
    }
}
