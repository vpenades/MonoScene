using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PBRMeshFactory : GLTFMeshFactory
    {
        #region lifecycle

        public PBRMeshFactory(GraphicsDevice device) : base(device) { }

        #endregion         

        protected override Effect ConvertMaterial(MaterialContent srcMaterial, bool isSkinned)
        {
            return PBREffectsFactory.CreatePBREffect(srcMaterial, isSkinned, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }        
    }
}
