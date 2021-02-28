using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    public class ClassicMeshFactory : DeviceMeshFactory
    {
        #region lifecycle

        public ClassicMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region API        

        protected override Effect CreateEffect(MaterialContent srcMaterial, bool mustSupportSkinning)
        {
            var classicMaterial = new MaterialContentConverter(srcMaterial, GetTexture);
            return classicMaterial.CreateEffect(Device, mustSupportSkinning);
        }

        #endregion        
    }
}
