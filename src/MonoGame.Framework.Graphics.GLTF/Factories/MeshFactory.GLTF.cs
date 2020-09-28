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

        protected GLTFMeshFactory(GraphicsDevice device) : base(device)
        {
            _TextureFactory = new GLTFTextureFactory(device);
            _SolidTextureFactory = new SolidColorTextureFactory(device);
        }

        private readonly GLTFTextureFactory _TextureFactory;
        private readonly SolidColorTextureFactory _SolidTextureFactory;

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

        #endregion

        #region texture helpers        

        protected Texture2D UseTexture(SharpGLTF.Schema2.Texture texture)
        {
            return _TextureFactory.UseTexture(texture.PrimaryImage.Content, texture.Name);
        }

        protected Texture2D UseTexture(Color color)
        {
            return _SolidTextureFactory.UseTexture(color);
        }

        protected SamplerState UseSampler(SharpGLTF.Schema2.TextureSampler sampler)
        {
            if (sampler == null) return SamplerState.LinearWrap;

            return _TextureFactory.UseSampler(sampler.WrapS.ToXna(), sampler.WrapS.ToXna());
        }

        #endregion

        #region gltf basic helpers

        protected GLTFMATERIAL GetDefaultMaterial()
        {
            if (_DummyModel == null)  // !=
            {
                _DummyModel = SharpGLTF.Schema2.ModelRoot.CreateModel();
                _DummyModel.CreateMaterial("Default");
            }

            return _DummyModel.LogicalMaterials[0];
        }

        

        protected static float GetAlphaLevel(GLTFMATERIAL srcMaterial)
        {
            if (srcMaterial.Alpha == SharpGLTF.Schema2.AlphaMode.OPAQUE) return 1;

            var baseColor = srcMaterial.FindChannel("BaseColor");

            if (baseColor == null) return 1;

            return baseColor.Value.Parameter.W;
        }

        protected static Vector3 GetDiffuseColor(GLTFMATERIAL srcMaterial)
        {
            var diffuse = srcMaterial.FindChannel("Diffuse");

            if (diffuse == null) diffuse = srcMaterial.FindChannel("BaseColor");

            if (diffuse == null) return Vector3.One;

            return new Vector3(diffuse.Value.Parameter.X, diffuse.Value.Parameter.Y, diffuse.Value.Parameter.Z);
        }

        protected static Vector3 GetSpecularColor(GLTFMATERIAL srcMaterial)
        {
            var mr = srcMaterial.FindChannel("MetallicRoughness");

            if (mr == null) return Vector3.One; // default value 16

            var diffuse = GetDiffuseColor(srcMaterial);
            var metallic = mr.Value.Parameter.X;
            var roughness = mr.Value.Parameter.Y;

            var k = Vector3.Zero;
            k += Vector3.Lerp(diffuse, Vector3.Zero, roughness);
            k += Vector3.Lerp(diffuse, Vector3.One, metallic);
            k *= 0.5f;

            return k;
        }

        protected static float GetSpecularPower(GLTFMATERIAL srcMaterial)
        {
            var mr = srcMaterial.FindChannel("MetallicRoughness");

            if (mr == null) return 16; // default value = 16

            var metallic = mr.Value.Parameter.X;
            var roughness = mr.Value.Parameter.Y;

            return 4 + 16 * metallic;
        }

        protected static Vector3 GeEmissiveColor(GLTFMATERIAL srcMaterial)
        {
            var emissive = srcMaterial.FindChannel("Emissive");

            if (emissive == null) return Vector3.Zero;

            return new Vector3(emissive.Value.Parameter.X, emissive.Value.Parameter.Y, emissive.Value.Parameter.Z);
        }

        protected Texture2D UseDiffuseTexture(GLTFMATERIAL srcMaterial)
        {
            var diffuse = srcMaterial.FindChannel("Diffuse");

            if (diffuse == null) diffuse = srcMaterial.FindChannel("BaseColor");
            if (diffuse == null) return null;
            if (diffuse.Value.Texture == null) return null;

            return UseTexture(diffuse.Value.Texture);
        }

        #endregion

        #region gltf advanced helpers

        protected void TransferChannel(EffectTexture2D.Scalar1 dst, GLTFMATERIAL src, string name, float defval)
        {
            dst.Texture = UseChannelTexture(src, name);
            dst.Sampler = UseChannelSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        protected void TransferChannel(EffectTexture2D.Scalar2 dst, GLTFMATERIAL src, string name, Vector2 defval)
        {
            dst.Texture = UseChannelTexture(src, name);
            dst.Sampler = UseChannelSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        protected void TransferChannel(EffectTexture2D.Scalar3 dst, GLTFMATERIAL src, string name, Vector3 defval)
        {
            dst.Texture = UseChannelTexture(src, name);
            dst.Sampler = UseChannelSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        protected void TransferChannel(EffectTexture2D.Scalar4 dst, GLTFMATERIAL src, string name, Vector4 defval)
        {
            dst.Texture = UseChannelTexture(src, name);
            dst.Sampler = UseChannelSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        protected float GetScaler(GLTFMATERIAL srcMaterial, string name, float defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return param.X;
        }

        protected (Vector3 u, Vector3 v) GetTransform(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return (Vector3.UnitX, Vector3.UnitY);

            if (channel.Value.TextureTransform == null) return (Vector3.UnitX, Vector3.UnitY);

            var S = System.Numerics.Matrix3x2.CreateScale(channel.Value.TextureTransform.Scale);
            var R = System.Numerics.Matrix3x2.CreateRotation(-channel.Value.TextureTransform.Rotation);
            var T = System.Numerics.Matrix3x2.CreateTranslation(channel.Value.TextureTransform.Offset);

            var X = S * R * T;

            return (new Vector3(X.M11, X.M21, X.M31), new Vector3(X.M12, X.M22, X.M32));
        }

        protected int GetTextureSet(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return 0;

            if (channel.Value.TextureTransform == null) return channel.Value.TextureCoordinate;

            return channel.Value.TextureTransform.TextureCoordinateOverride ?? channel.Value.TextureCoordinate;
        }

        protected Vector2 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector2 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector2(param.X, param.Y);
        }

        protected Vector3 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector3 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector3(param.X, param.Y, param.Z);
        }

        protected Vector4 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector4 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector4(param.X, param.Y, param.Z, param.W);
        }

        protected Texture2D UseChannelTexture(GLTFMATERIAL srcMaterial, string channelName)
        {
            var channel = srcMaterial.FindChannel(channelName);

            if (!channel.HasValue) return null;
            if (channel.Value.Texture == null) return null;
            if (channel.Value.Texture.PrimaryImage == null) return null;
            if (channel.Value.Texture.PrimaryImage.Content.IsEmpty) return null;

            return UseTexture(channel.Value.Texture);
        }

        protected SamplerState UseChannelSampler(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return null;
            if (channel.Value.Texture == null) return null;

            return UseSampler(channel.Value.TextureSampler);
        }

        #endregion
    }
}
