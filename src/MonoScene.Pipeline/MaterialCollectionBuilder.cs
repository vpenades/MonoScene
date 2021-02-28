using System;
using System.Collections.Generic;
using System.Text;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    public abstract class MaterialCollectionBuilder<TMaterialKey>
    {
        #region data

        private readonly TMaterialKey _DefaultMaterial;

        private readonly Dictionary<TMaterialKey, int> _MaterialMapping = new Dictionary<TMaterialKey, int>();        

        internal readonly List<Byte[]> _SharedTextures = new List<byte[]>();
        internal readonly List<MaterialContent> _Materials = new List<MaterialContent>();

        #endregion

        #region API

        public int UseTexture(Byte[] texture)
        {
            if (texture == null) return -1;
            if (texture.Length == 0) return -1;

            for (int i = 0; i < _SharedTextures.Count; ++i)
            {
                if (texture.AsSpan().SequenceEqual(_SharedTextures[i])) return i;
            }

            _SharedTextures.Add(texture);

            return _SharedTextures.Count - 1;
        }

        public int UseMaterial(TMaterialKey srcMaterial)
        {
            if (srcMaterial == null) srcMaterial = _DefaultMaterial;

            if (_MaterialMapping.TryGetValue(srcMaterial, out int index)) return index;

            index = _Materials.Count;

            var dstMaterial = Convert(srcMaterial);
            if (dstMaterial == null) throw new ArgumentNullException(nameof(srcMaterial));
            if (_Materials.Contains(dstMaterial)) throw new InvalidOperationException();

            _Materials.Add(dstMaterial);
            _MaterialMapping[srcMaterial] = index;

            return index;            
        }       

        protected abstract MaterialContent Convert(TMaterialKey srcMaterial);

        public MaterialCollectionContent CreateContent()
        {
            var dst = new MaterialCollectionContent();
            dst._SharedTextures.AddRange(_SharedTextures);
            dst._Materials.AddRange(_Materials);
            return dst;
        }

        #endregion
    }
}
