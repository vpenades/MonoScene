using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using XY = Microsoft.Xna.Framework.Vector2;
using XYZ = Microsoft.Xna.Framework.Vector3;
using XYZW = Microsoft.Xna.Framework.Vector4;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
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

        private MeshPrimitiveMaterial _DefaultMaterial;
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
        private GraphicsResourceTracker _Disposables;

        private ImageFileTextureFactory _TextureFactory;

        #endregion

        #region properties
        protected GraphicsDevice Device => _Device;

        #endregion

        #region API

        protected TextureFactory<Byte[]> FileContentTextureFactory => _TextureFactory;

        public MeshCollection CreateMeshCollection(IEnumerable<IMeshDecoder<TMaterial>> srcMeshes)
        {
            if (srcMeshes == null) throw new ArgumentNullException(nameof(srcMeshes));
            
            _Disposables = new GraphicsResourceTracker();            

            int meshIndex = 0;

            var meshPrimitiveBuilder = new MeshPrimitiveBuilder();

            // aggregate the primitives of all meshes, so the builder can determine the shared resources

            foreach (var srcMesh in srcMeshes)
            {
                foreach (var srcPrim in srcMesh.Primitives)
                {
                    if (!srcPrim.TriangleIndices.Any()) continue;

                    Type vertexType = GetPreferredVertexType(srcPrim);

                    MeshPrimitiveMaterial material = null;

                    // we cannot set a Null Key for a dictionary, so we need to handle null (default) materials separately
                    if (srcPrim.Material == null) 
                    {
                        if (_DefaultMaterial != null) material = _DefaultMaterial;
                        else
                        {
                            material = ConvertMaterial(null, srcPrim.JointsWeightsCount > 0);
                            if (material == null) throw new NullReferenceException("NULL Material conversion failed");
                            _DefaultMaterial = material;
                        }
                    }

                    // for all other defined materials we follow the dictionary path
                    else
                    {
                        if (!_Materials.TryGetValue(srcPrim.Material, out material))
                        {
                            material = ConvertMaterial(srcPrim.Material, srcPrim.JointsWeightsCount > 0);
                            if (material == null) throw new NullReferenceException("Material conversion failed");
                            _Materials[srcPrim.Material] = material;
                        }
                    }

                    meshPrimitiveBuilder.AppendMeshPrimitive(meshIndex, vertexType, srcPrim, material.Effect, material.Blend, material.DoubleSided);
                }

                ++meshIndex;
            }

            // Create the runtime meshes

            var dstMeshes = meshPrimitiveBuilder.CreateRuntimeMeshes(_Device, _Disposables)
                .OrderBy(item => item.Key)
                .Select(item => item.Value)
                .ToArray();

            _Materials.Clear();

            return new MeshCollection(dstMeshes, _Disposables.Disposables.ToArray());
        }

        protected virtual Type GetPreferredVertexType(IMeshPrimitiveDecoder<TMaterial> srcPrim)
        {
            return srcPrim.JointsWeightsCount > 0 ? typeof(VertexSkinned) : typeof(VertexRigid);
        }

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

        #endregion
    }

    public class PBRMeshFactory : MeshFactory
    {
        #region lifecycle

        public PBRMeshFactory(GraphicsDevice device) : base(device)
        {
        }

        #endregion
        protected override Effect CreateEffect(MaterialContent srcMaterial, bool isSkinned)
        {
            return PBREffectsFactory.CreatePBREffect(srcMaterial, isSkinned, Device, tobj => FileContentTextureFactory.UseTexture(tobj as Byte[]));
        }
    }

    public class ClassicMeshFactory : MeshFactory
    {
        #region lifecycle

        public ClassicMeshFactory(GraphicsDevice device) : base(device)
        {
        }

        #endregion

        #region API

        protected override Type GetPreferredVertexType(IMeshPrimitiveDecoder<MaterialContent> srcPrim)
        {
            return srcPrim.JointsWeightsCount > 0 ? typeof(VertexBasicSkinned) : typeof(VertexPositionNormalTexture);
        }

        protected override Effect CreateEffect(MaterialContent srcMaterial, bool mustSupportSkinning)
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
