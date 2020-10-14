using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class AssimpMeshFactory : MeshFactory<Assimp.Material>
    {
        #region lifecycle

        protected AssimpMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion        

        #region mesh converters

        public MeshCollection CreateMeshCollection(IEnumerable<Assimp.Mesh> logicalMeshes, Func<int, Assimp.Material> mats)
        {
            var meshes = logicalMeshes
                .Select(item => new _MeshDecoder<Assimp.Material>(item, mats(item.MaterialIndex)))
                .Cast<IMeshDecoder<Assimp.Material>>()
                .ToArray();

            return CreateMeshCollection(meshes);
        }

        protected override MeshPrimitiveMaterial ConvertMaterial(Assimp.Material srcMaterial, bool isSkinned)
        {
            var matContent = srcMaterial.ToXna();

            var effect = ConvertMaterial(matContent, isSkinned);

            var material = new MeshPrimitiveMaterial();

            material.Effect = effect;
            material.DoubleSided = srcMaterial.HasTwoSided;
            material.Blend = matContent.Mode == MaterialBlendMode.Blend ? BlendState.NonPremultiplied : BlendState.Opaque;

            return material;
        }

        protected abstract Effect ConvertMaterial(MaterialContent srcMaterial, bool isSkinned);

        #endregion        
    }

    public class BasicMeshFactory : AssimpMeshFactory
    {
        #region lifecycle

        public BasicMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region API

        protected override Type GetPreferredVertexType(IMeshPrimitiveDecoder<Assimp.Material> srcPrim)
        {
            return srcPrim.JointsWeightsCount > 0 ? typeof(VertexBasicSkinned) : typeof(VertexPositionNormalTexture);
        }

        protected override Effect ConvertMaterial(MaterialContent srcMaterial, bool mustSupportSkinning)
        {
            return PBREffectsFactory.CreateClassicEffect(srcMaterial, mustSupportSkinning, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }

        #endregion

        #region vertex types

        struct VertexBasicSkinned : IVertexType
        {
            #region static

            private static VertexDeclaration _VDecl = CreateVertexDeclaration();

            public static VertexDeclaration CreateVertexDeclaration()
            {
                int offset = 0;

                var a = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
                offset += 3 * 4;

                var b = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
                offset += 3 * 4;

                var c = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
                offset += 2 * 4;

                var d = new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0);
                offset += 4 * 1;

                var e = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0);
                offset += 4 * 4;

                return new VertexDeclaration(a, b, c, d, e);
            }

            #endregion

            #region data

            public VertexDeclaration VertexDeclaration => _VDecl;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Framework.Graphics.PackedVector.Byte4 BlendIndices;
            public Vector4 BlendWeight;

            #endregion
        }

        #endregion
    }

    public class PBRMeshFactory : AssimpMeshFactory
    {
        #region lifecycle

        public PBRMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region API
        
        protected override Effect ConvertMaterial(MaterialContent srcMaterial, bool mustSupportSkinning)
        {
            return PBREffectsFactory.CreatePBREffect(srcMaterial, mustSupportSkinning, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }

        #endregion        
    }
}
