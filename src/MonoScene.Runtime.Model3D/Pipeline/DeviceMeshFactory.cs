using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Helper class to process <see cref="MaterialCollectionContent"/> and<br/>
    /// <see cref="MeshCollectionContent"/> content source into a GPU ready<br/>
    /// <see cref="MeshCollection"/>.
    /// </summary>
    /// <remarks>
    /// In normal circumstances it could be easy to simply convert all the<br/>
    /// textures within a <see cref="MaterialCollectionContent"/>. But depending<br/>
    /// of the shader, we might not need all the source textures, so we can only<br/>
    /// instantiate the textures as we traverse the graph, so only what's actually<br/>
    /// used will consume GPU resources.
    /// </remarks>
    public abstract class DeviceMeshFactory
    {
        #region lifecycle

        public DeviceMeshFactory(GraphicsDevice device)            
        {
            _Device = device;
            _TextureFactory = new ImageFileTextureFactory(device);
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;

        /// <summary>
        /// Gathers all disposable resources shared by the collection of meshes:
        /// - <see cref="VertexBuffer"/>
        /// - <see cref="IndexBuffer"/>
        /// - <see cref="Texture2D"/>
        /// - <see cref="Effect"/>
        /// - Custom <see cref="BlendState"/>
        /// - Custom <see cref="SamplerState"/>
        /// </summary>
        private GraphicsResourceTracker _Disposables;

        private IReadOnlyList<TextureContent> _TextureContent;

        private TextureFactory<Byte[]> _TextureFactory;

        #endregion

        #region properties
        protected GraphicsDevice Device => _Device;

        #endregion

        #region overridable API

        protected Texture2D GetTexture(int index)
        {
            if (index < 0) return null;

            if (index == 65536 * 256) return _TextureFactory.UseWhiteImage();

            var content = _TextureContent[index];

            Texture2D tex = null;

            if (content is ImageContent contentImg)
            {
                tex = _TextureFactory.UseTexture(contentImg.Data);
            }            

            _Disposables.AddDisposable(tex);

            return tex;
        }

        /// <summary>
        /// Creates a new effect from <paramref name="srcMaterial"/>.
        /// </summary>
        /// <param name="srcMaterial">The <see cref="MaterialContent"/> to use as source.</param>
        /// <param name="meshIsSkinned">
        /// If it's true, the returned <see cref="Effect"/><br/>
        /// must implement <see cref="IEffectBones"/>.
        /// </param>
        /// <returns></returns>
        protected abstract Effect CreateEffect(MaterialContent srcMaterial, bool meshIsSkinned);

        #endregion

        #region API        

        public MeshCollection CreateMeshCollection(MaterialCollectionContent srcMaterials, MeshCollectionContent srcMeshes)
        {
            // check arguments

            if (srcMaterials == null) throw new ArgumentNullException(nameof(srcMaterials));
            if (srcMeshes == null) throw new ArgumentNullException(nameof(srcMeshes));

            foreach(var srcPart in srcMeshes.Meshes.SelectMany(item => item.Parts))
            {
                if (srcPart.MaterialIndex < 0 || srcPart.MaterialIndex >= srcMaterials.Materials.Count) throw new ArgumentOutOfRangeException(nameof(srcMeshes), "MaterialIndex");
            }

            // initialize internals

            _TextureContent = srcMaterials.Textures;

            _Disposables = new GraphicsResourceTracker();

            // instantiate vertex and index buffers

            var vertexBuffers = srcMeshes.SharedVertexBuffers
                .Select(item => item.CreateVertexBuffer(Device))
                .ToArray();

            var indexBuffers = srcMeshes.SharedIndexBuffers
                .Select(item => item.CreateIndexBuffer(Device))
                .ToArray();            

            _Disposables.AddDisposables(vertexBuffers);
            _Disposables.AddDisposables(indexBuffers);

            // instantiate effects lambda

            // There isn't an exact match between content materials and effects,
            // because depending on the behaviour a MaterialContent might be used
            // on rigid and skinned meshes alike, so we have to create the
            // appropiate effect on demand.

            var rigidEffects = new Dictionary<MaterialContent, Effect>();
            var skinnedEffects = new Dictionary<MaterialContent, Effect>();

            Effect useEffect(MaterialContent srcMaterial, bool meshIsSkinned)
            {
                var dict = meshIsSkinned ? skinnedEffects : rigidEffects;
                if (dict.TryGetValue(srcMaterial, out Effect effect)) return effect;
                effect = CreateEffect(srcMaterial, meshIsSkinned);

                if (effect == null) throw new NullReferenceException(nameof(CreateEffect));
                if (meshIsSkinned && !(effect is IEffectBones)) throw new InvalidCastException($"Effect must implement IEffectBones");

                dict[srcMaterial] = effect;

                _Disposables.AddDisposable(effect);
                return effect;
            }

            // coalesce meshes

            var dstMeshes = new List<Mesh>();

            foreach(var srcMesh in srcMeshes.Meshes)
            {
                var dstMesh = new Mesh(Device);

                foreach(var srcPart in srcMesh.Parts)
                {
                    var srcMaterial = srcMaterials.Materials[srcPart.MaterialIndex];

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
}
