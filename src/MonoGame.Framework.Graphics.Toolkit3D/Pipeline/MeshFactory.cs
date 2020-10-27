using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using XY = Microsoft.Xna.Framework.Vector2;
using XYZ = Microsoft.Xna.Framework.Vector3;
using XYZW = Microsoft.Xna.Framework.Vector4;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    public abstract class MeshFactory<TMaterial>
        where TMaterial : class
    {
        #region lifecycle

        public MeshFactory(GraphicsDevice device)
        {
            _Device = device;

            _TextureFactory = new ImageFileTextureFactory(_Device);
        }

        #endregion

        #region data

        private readonly GraphicsDevice _Device;
        
        private readonly Dictionary<TMaterial, MeshPrimitiveMaterial> _Materials = new Dictionary<TMaterial, MeshPrimitiveMaterial>();        

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

        protected abstract MeshPrimitiveMaterial ConvertMaterial(TMaterial material, bool mustSupportSkinning);

        #endregion        
    }

    public abstract class MeshFactory : MeshFactory<MaterialContent>
    {
        #region lifecycle

        public MeshFactory(GraphicsDevice device) : base(device)
        {
        }

        #endregion

        #region API

        protected override MeshPrimitiveMaterial ConvertMaterial(MaterialContent srcMaterial, bool isSkinned)
        {
            var effect = CreateEffect(srcMaterial, isSkinned);

            var material = new MeshPrimitiveMaterial();

            material.Effect = effect;
            material.DoubleSided = srcMaterial.DoubleSided;
            material.Blend = srcMaterial.Mode == MaterialBlendMode.Blend ? BlendState.NonPremultiplied : BlendState.Opaque;

            return material;
        }

        protected abstract Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned);

        public static IEnumerable<(Vector3 A,Vector3 B,Vector3 C)> EvaluateTriangles(ModelInstance instance, IReadOnlyList<IMeshDecoder<MaterialContent>> meshes)
        {
            foreach(var drawable in instance.DrawableInstances)
            {
                var srcMesh = meshes[drawable.Template.MeshIndex];
                var srcXfrm = drawable.Transform;

                foreach(var prim in srcMesh.Primitives)
                {
                    foreach (var (idx0, idx1, idx2) in prim.TriangleIndices)
                    {
                        var pos0 = prim.GetPosition(idx0);
                        var pos1 = prim.GetPosition(idx1);
                        var pos2 = prim.GetPosition(idx2);

                        var sjw0 = prim.GetSkinWeights(idx0);
                        var sjw1 = prim.GetSkinWeights(idx1);
                        var sjw2 = prim.GetSkinWeights(idx2);

                        var a = srcXfrm.TransformPosition(pos0, null, sjw0);
                        var b = srcXfrm.TransformPosition(pos1, null, sjw1);
                        var c = srcXfrm.TransformPosition(pos2, null, sjw2);

                        yield return (a, b, c);
                    }
                }
            }
        }

        public static BoundingSphere EvaluateBoundingSphere(ModelInstance instance, IReadOnlyList<IMeshDecoder<MaterialContent>> meshes)
        {
            var triangles = EvaluateTriangles(instance, meshes)
                .SelectMany(item => new[] { item.A, item.B, item.C });

            return BoundingSphere.CreateFromPoints(triangles);
        }

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

    public class PBRMeshFactory : MeshFactory
    {
        public PBRMeshFactory(GraphicsDevice device)
            : base(device) { }
        
        protected override Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned)
        {
            return PBREffectsFactory.CreatePBREffect(srcMaterial, isSkinned, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }
    }

    public class ClassicMeshFactory : MeshFactory
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

    public class MeshPrimitiveMaterial
    {
        public Effect Effect;
        public BlendState Blend;
        public bool DoubleSided;

        public class MeshFactory : MeshFactory<MeshPrimitiveMaterial>
        {
            public MeshFactory(GraphicsDevice device) : base(device) { }

            protected override MeshPrimitiveMaterial ConvertMaterial(MeshPrimitiveMaterial material, bool mustSupportSkinning)
            {
                return material;
            }
        }
    }
}
