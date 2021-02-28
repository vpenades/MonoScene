using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    public class MaterialCollectionContent
    {
        #region data

        internal readonly List<Byte[]> _SharedTextures = new List<Byte[]>();

        internal readonly List<MaterialContent> _Materials = new List<MaterialContent>();

        #endregion

        #region API

        public IReadOnlyList<Byte[]> Textures => _SharedTextures;

        public IReadOnlyList<MaterialContent> Materials => _Materials;        

        #endregion
    }
}
