using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class GLTFMeshFactory : MeshFactory<GLTFMATERIAL>
    {
        #region lifecycle

        protected GLTFMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region data        

        private static SharpGLTF.Schema2.ModelRoot _DummyModel;

        #endregion

        #region mesh converters

        public MeshCollection CreateMeshCollection(IEnumerable<SharpGLTF.Schema2.Mesh> logicalMeshes)
        {
            var meshes = logicalMeshes
                .Select(item => new _MeshDecoder<GLTFMATERIAL>( SharpGLTF.Runtime.MeshDecoder.Decode(item)))
                .Cast<IMeshDecoder<GLTFMATERIAL>>()
                .ToArray();

            return CreateMeshCollection(meshes);
        }

        protected override MeshPrimitiveMaterial ConvertMaterial(GLTFMATERIAL srcMaterial, bool isSkinned)
        {
            if (srcMaterial == null) srcMaterial = GetDefaultMaterial();

            var matContent = srcMaterial.ToXna();

            var effect = ConvertMaterial(matContent, isSkinned);

            var material = new MeshPrimitiveMaterial();

            material.Effect = effect;
            material.DoubleSided = srcMaterial.DoubleSided;
            material.Blend = matContent.Mode == MaterialBlendMode.Blend ? BlendState.NonPremultiplied : BlendState.Opaque;

            return material;
        }

        protected abstract Effect ConvertMaterial(MaterialContent srcMaterial, bool isSkinned);

        #endregion

        #region helpers

        protected GLTFMATERIAL GetDefaultMaterial()
        {
            if (_DummyModel == null)  // !=
            {
                _DummyModel = SharpGLTF.Schema2.ModelRoot.CreateModel();
                _DummyModel.CreateMaterial("Default");
            }

            return _DummyModel.LogicalMaterials[0];
        }        

        #endregion        
    }
}
