using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Represents a 3D model within <see cref="ModelCollectionContent"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="ModelContent"/> just contains<br/>
    /// a batch of <see cref="DrawableContent"/> commands.<br/>
    /// Actual geometry, materials and texture data is stored<br/>
    /// in <see cref="ModelCollectionContent"/> and might be<br/>
    /// shared by other <see cref="ModelContent"/> elements<br/>
    /// within the collection.
    /// </remarks>
    public class ModelContent : BaseContent
    {
        #region lifecycle        

        public ModelContent(int armatureIndex, IEnumerable<DrawableContent> drawables)            
        {
            ArmatureLogicalIndex = armatureIndex;
            _DrawableReferences.AddRange(drawables);
        }

        #endregion

        #region data

        private readonly List<DrawableContent> _DrawableReferences = new List<DrawableContent>();

        #endregion

        #region properties
        public int ArmatureLogicalIndex { get; set; }
        public IReadOnlyList<DrawableContent> DrawableReferences => _DrawableReferences;
        public Microsoft.Xna.Framework.BoundingSphere ModelBounds { get; set; }        

        #endregion        
    }
}
