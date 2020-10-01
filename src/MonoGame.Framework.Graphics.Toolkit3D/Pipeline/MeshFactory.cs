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

        #endregion

        #region properties
        protected GraphicsDevice Device => _Device;

        #endregion

        #region API

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
