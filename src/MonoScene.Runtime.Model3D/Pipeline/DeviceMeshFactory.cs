using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

using XNAV3 = Microsoft.Xna.Framework.Vector3;

namespace MonoScene.Graphics.Pipeline
{
    public abstract class DeviceMeshFactory<TMaterial>
        where TMaterial : class
    {
        #region lifecycle

        public DeviceMeshFactory(GraphicsDevice device)
        {
            _Device = device;

            _TextureFactory = new ImageFileTextureFactory(_Device);
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;
        
        private readonly Dictionary<TMaterial, DeviceMeshPrimitiveMaterial> _Materials = new Dictionary<TMaterial, DeviceMeshPrimitiveMaterial>();        

        /// <summary>
        /// Gathers all disposable resources shared by the collection of meshes:
        /// - <see cref="VertexBuffer"/>
        /// - <see cref="IndexBuffer"/>
        /// - <see cref="Texture2D"/>
        /// - <see cref="Effect"/>
        /// - Custom <see cref="BlendState"/>
        /// - Custom <see cref="SamplerState"/>
        /// </summary>
        internal GraphicsResourceTracker _Disposables;

        private ImageFileTextureFactory _TextureFactory;

        #endregion

        #region properties
        protected GraphicsDevice Device => _Device;

        #endregion

        #region API

        protected TextureFactory<Byte[]> FileContentTextureFactory => _TextureFactory;        

        protected abstract DeviceMeshPrimitiveMaterial ConvertMaterial(TMaterial material, bool mustSupportSkinning);

        #endregion        
    }

    public abstract class DeviceMeshFactory : DeviceMeshFactory<MaterialContent>
    {
        #region lifecycle

        public DeviceMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region overridable API

        protected override DeviceMeshPrimitiveMaterial ConvertMaterial(MaterialContent srcMaterial, bool isSkinned)
        {
            var effect = CreateEffect(srcMaterial, isSkinned);

            var material = new DeviceMeshPrimitiveMaterial();

            material.Effect = effect;
            material.DoubleSided = srcMaterial.DoubleSided;
            material.Blend = srcMaterial.Mode == MaterialBlendMode.Blend ? BlendState.NonPremultiplied : BlendState.Opaque;

            return material;
        }

        protected abstract Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned);

        #endregion

        #region static API        

        public MeshCollection CreateMeshCollection(MeshCollectionContent srcMeshes)
        {
            if (srcMeshes == null) throw new ArgumentNullException(nameof(srcMeshes));
            _Disposables = new GraphicsResourceTracker();

            var vertexBuffers = srcMeshes.SharedVertexBuffers
                .Select(item => item.CreateVertexBuffer(Device))
                .ToArray();

            var indexBuffers = srcMeshes.SharedIndexBuffers
                .Select(item => item.CreateIndexBuffer(Device))
                .ToArray();            

            _Disposables.AddDisposables(vertexBuffers);
            _Disposables.AddDisposables(indexBuffers);

            // There isn't an exact match between content materials and effects,
            // because depending on the effects we choose, we have to split
            // between effects supporting skinning or not.            
            var rigidEffects = new Dictionary<MaterialContent, Effect>();
            var skinnedEffects = new Dictionary<MaterialContent, Effect>();

            Effect useEffect(MaterialContent srcMaterial, bool isSkinned)
            {
                var dict = isSkinned ? skinnedEffects : rigidEffects;
                if (dict.TryGetValue(srcMaterial, out Effect effect)) return effect;
                dict[srcMaterial] = effect = CreateEffect(srcMaterial, isSkinned);
                _Disposables.AddDisposable(effect);
                return effect;
            }

            var dstMeshes = new List<Mesh>();

            foreach(var srcMesh in srcMeshes.Meshes)
            {
                var dstMesh = new Mesh(Device);

                foreach(var srcPart in srcMesh.Parts)
                {
                    var srcMaterial = srcMeshes.SharedMaterials[srcPart.MaterialIndex];
                    var hasSkin = srcMeshes.SharedVertexBuffers[srcPart.Geometry.VertexBufferIndex].HasSkinning;

                    var dstGeometry = MeshTriangles.CreateFrom(srcPart.Geometry, vertexBuffers, indexBuffers);
                    dstGeometry.SetCullingStates(srcMaterial.DoubleSided);                    

                    var dstPart = dstMesh.CreateMeshPart();
                    dstPart.Effect = useEffect(srcMaterial, hasSkin);
                    dstPart.Blending = srcMaterial.Mode == MaterialBlendMode.Blend ? BlendState.NonPremultiplied : BlendState.Opaque;
                    dstPart.Geometry = dstGeometry;
                }

                dstMeshes.Add(dstMesh);
            }

            return new MeshCollection(dstMeshes.ToArray(), _Disposables.Disposables.ToArray());
        }

        #endregion
    }

    public class PBRMeshFactory : DeviceMeshFactory
    {
        public PBRMeshFactory(GraphicsDevice device)
            : base(device) { }
        
        protected override Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned)
        {
            return PBREffectsFactory.CreatePBREffect(srcMaterial, isSkinned, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }
    }

    public class ClassicMeshFactory : DeviceMeshFactory
    {
        #region lifecycle

        public ClassicMeshFactory(GraphicsDevice device)
            : base(device) { }

        #endregion

        #region API        

        protected override Effect CreateEffect(MaterialContent srcMaterial, bool mustSupportSkinning)
        {
            return PBREffectsFactory.CreateClassicEffect(srcMaterial, mustSupportSkinning, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }

        #endregion        
    }

    public class DeviceMeshPrimitiveMaterial
    {
        public Effect Effect;
        public BlendState Blend;
        public bool DoubleSided;

        public class MeshFactory : DeviceMeshFactory<DeviceMeshPrimitiveMaterial>
        {
            public MeshFactory(GraphicsDevice device) : base(device) { }

            protected override DeviceMeshPrimitiveMaterial ConvertMaterial(DeviceMeshPrimitiveMaterial material, bool mustSupportSkinning)
            {
                return material;
            }
        }
    }
}
