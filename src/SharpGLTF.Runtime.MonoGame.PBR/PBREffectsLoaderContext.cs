using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;
using SharpGLTF.Runtime.Effects;
using SharpGLTF.Schema2;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace SharpGLTF.Runtime
{
    public class PBREffectsLoaderContext : LoaderContext
    {
        #region lifecycle

        public static LoaderContext CreateLoaderContext(GraphicsDevice device)
        {
            if (device.GraphicsProfile == GraphicsProfile.HiDef) return new PBREffectsLoaderContext(device);

            return new BasicEffectsLoaderContext(device);
        }

        public PBREffectsLoaderContext(GraphicsDevice device) : base(device)
        {
            Resources.GenerateDotTextures(device);
        }

        #endregion

        #region effects creation        

        protected override Effect CreateEffect(GLTFMATERIAL srcMaterial, bool isSkinned)
        {
            Effects.PBREffect effect = null;

            if (srcMaterial.FindChannel("SpecularGlossiness") != null)
            {
                var xeffect = new Effects.SpecularGlossinessEffect(this.Device);
                effect = xeffect;

                xeffect.DiffuseScale = GetScaler(srcMaterial, "Diffuse", Vector4.One);
                xeffect.DiffuseMap = UseTexture(srcMaterial, "Diffuse");

                xeffect.SpecularGlossinessScale = GetScaler(srcMaterial, "SpecularGlossiness", Vector4.Zero);
                xeffect.SpecularGlossinessMap = UseTexture(srcMaterial, "SpecularGlossiness");
            }
            else
            {
                var xeffect = new Effects.MetallicRoughnessEffect(this.Device);
                effect = xeffect;

                xeffect.BaseColorScale = GetScaler(srcMaterial, "BaseColor", Vector4.One);
                xeffect.BaseColorMap = UseTexture(srcMaterial, "BaseColor");

                xeffect.MetalRoughnessScale = GetScaler(srcMaterial, "MetallicRoughness", Vector2.Zero);
                xeffect.MetalRoughnessMap = UseTexture(srcMaterial, "MetallicRoughness");
            }            

            effect.NormalScale = GetScaler(srcMaterial, "Normal", Vector4.Zero).X;
            effect.NormalMap = UseTexture(srcMaterial, "Normal");            

            effect.OcclusionScale = GetScaler(srcMaterial, "Occlusion", Vector4.Zero).X;
            effect.OcclusionMap = UseTexture(srcMaterial, "Occlusion");

            return effect;
        }

        #endregion

        #region meshes creation

        protected override void WriteMeshPrimitive(MeshPrimitiveReader srcPrimitive, Effect effect)
        {
            if (srcPrimitive.IsSkinned) WriteMeshPrimitive<VertexSkinned>(effect, srcPrimitive);
            else WriteMeshPrimitive<VertexRigid>(effect, srcPrimitive);
        }

        #endregion

        #region gltf helpers        

        private Vector2 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector2 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector2(param.X, param.Y);
        }

        private Vector4 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector4 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector4(param.X, param.Y, param.Z, param.W);
        }

        private Texture2D UseTexture(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return null;
            if (channel.Value.Texture == null) return null;
            if (channel.Value.Texture.PrimaryImage == null) return null;
            if (channel.Value.Texture.PrimaryImage.Content.IsEmpty) return null;

            return UseTexture(channel.Value, null);
        }
        
        #endregion

        #region vertex types

        struct VertexRigid : IVertexType
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

                var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
                offset += 4 * 4;

                var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
                offset += 4 * 1;

                var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
                offset += 2 * 4;                

                return new VertexDeclaration(a, b, c, d, e);
            }

            #endregion

            #region data

            public VertexDeclaration VertexDeclaration => _VDecl;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedByte4 Color;
            public Vector2 TextureCoordinate;            

            #endregion
        }

        struct VertexSkinned : IVertexType
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

                var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
                offset += 4 * 4;

                var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
                offset += 4 * 1;

                var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
                offset += 2 * 4;

                var f = new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0);
                offset += 4 * 1;

                var g = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0);
                offset += 4 * 4;

                return new VertexDeclaration(a, b, c, d, e, f, g);
            }

            #endregion

            #region data

            public VertexDeclaration VertexDeclaration => _VDecl;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedByte4 Color;
            public Vector2 TextureCoordinate;
            public Microsoft.Xna.Framework.Graphics.PackedVector.Byte4 BlendIndices;
            public Vector4 BlendWeight;

            #endregion
        }

        #endregion
    }
}