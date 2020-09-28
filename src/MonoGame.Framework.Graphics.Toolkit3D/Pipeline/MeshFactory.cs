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
            _Disposables = new GraphicsResourceTracker();            

            int meshIndex = 0;

            var meshPrimitiveBuilder = new MeshPrimitiveBuilder();

            // aggregate the primitives of all meshes, so the builder can determine the shared resources

            foreach (var srcMesh in srcMeshes)
            {
                foreach (var srcPrim in srcMesh.Primitives)
                {
                    Type vertexType = GetPreferredVertexType(srcPrim);

                    if (!_Materials.TryGetValue(srcPrim.Material, out MeshPrimitiveMaterial material))
                    {
                        material = ConvertMaterial(srcPrim.Material, srcPrim.JointsWeightsCount > 0);
                        if (material == null) throw new NullReferenceException("Material conversion failed");
                        _Materials[srcPrim.Material] = material;
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
