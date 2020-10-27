using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="MeshContent"/> objects and their associated resources.
    /// </summary>
    public class MeshCollectionContent
    {
        #region lifecycle

        public static MeshCollectionContent CreateFromMeshes(IReadOnlyList<IMeshDecoder<MaterialContent>> srcMeshes)
        {
            var builder = new MeshCollectionBuilder();

            for (int i = 0; i < srcMeshes.Count; ++i)
            {
                var srcMesh = srcMeshes[i];
                var hasSkin = srcMesh.Primitives.Any(item => item.JointsWeightsCount > 0);

                builder.AppendMesh(srcMesh);
            }

            var dstMeshes = new MeshCollectionContent();
            
            dstMeshes._SharedVertexBuffers.AddRange(builder._VertexBuffers);
            dstMeshes._SharedIndexBuffers.AddRange(builder._IndexBuffers);
            dstMeshes._SharedMaterials.AddRange(builder._Materials);
            dstMeshes._Meshes.AddRange(builder._Meshes);

            return dstMeshes;
        }

        #endregion

        #region data

        private readonly List<VertexBufferContent> _SharedVertexBuffers = new List<VertexBufferContent>();
        private readonly List<IndexBufferContent> _SharedIndexBuffers = new List<IndexBufferContent>();
        private readonly List<MaterialContent> _SharedMaterials = new List<MaterialContent>();

        private readonly List<MeshContent> _Meshes = new List<MeshContent>();

        #endregion

        #region properties

        public IReadOnlyList<VertexBufferContent> SharedVertexBuffers => _SharedVertexBuffers;
        public IReadOnlyList<IndexBufferContent> SharedIndexBuffers => _SharedIndexBuffers;
        public IReadOnlyList<MaterialContent> SharedMaterials => _SharedMaterials;
        public IReadOnlyList<MeshContent> Meshes => _Meshes;

        #endregion
    }

    /// <summary>
    /// Helper class used to coalesce mesh resources and build a <see cref="MeshCollectionContent"/>
    /// </summary>
    class MeshCollectionBuilder
    {
        #region data

        // shared buffers
        internal readonly List<VertexBufferContent> _VertexBuffers = new List<VertexBufferContent>();
        internal readonly List<IndexBufferContent> _IndexBuffers = new List<IndexBufferContent>();        
        internal readonly List<MaterialContent> _Materials = new List<MaterialContent>();
        internal readonly List<MeshContent> _Meshes = new List<MeshContent>();

        #endregion

        #region API        

        private int _UseMaterialIndex(MaterialContent material)
        {
            var idx = _Materials.IndexOf(material);
            if (idx >= 0) return idx;

            _Materials.Add(material);
            return _Materials.Count - 1;
        }        

        private int _UseVertexBuffer(VertexDeclaration vdecl)
        {
            var vbIndex = _VertexBuffers.FindIndex(item => item.IsCompatibleWith(vdecl));
            if (vbIndex >= 0) return vbIndex;

            _VertexBuffers.Add(new VertexBufferContent());
            return _VertexBuffers.Count - 1;
        }

        private int _UseIndexBuffer()
        {
            var ibIndex = _IndexBuffers.FindIndex(item => true);
            if (ibIndex >= 0) return ibIndex;
            
            _IndexBuffers.Add(new IndexBufferContent());
            return _IndexBuffers.Count -1;
        }
        public void AppendMesh(IMeshDecoder<MaterialContent> srcMesh)            
        {
            var dstMesh = new MeshContent();

            foreach (var prim in srcMesh.Primitives)
            {
                var vdecl = prim.GetVertexDeclaration();

                int vbIndex = _UseVertexBuffer(vdecl);
                int ibIndex = _UseIndexBuffer();

                var geometry = CreateGeometry(vbIndex, ibIndex, prim, vdecl);
                dstMesh.AddMeshPart(geometry, _UseMaterialIndex(prim.Material));
            }

            _Meshes.Add(dstMesh);
        }

        private MeshGeometryContent CreateGeometry(int vbIndex, int ibIndex, IMeshPrimitiveDecoder<MaterialContent> primitive, VertexDeclaration vdecl)            
        {
            var partVertices = primitive.ToXnaVertices(vdecl);
            var partTriangles = primitive.TriangleIndices.ToList();

            var vRange = _VertexBuffers[vbIndex].AddVertices(partVertices, vdecl);
            var iRange = _IndexBuffers[ibIndex].AddTriangleIndices(partTriangles);

            var geometry = new MeshGeometryContent();
            geometry.SetVertices(vbIndex, vRange.Offset, vRange.Count);
            geometry.SetIndices(ibIndex, iRange.Offset, iRange.Count, partTriangles.Count);

            return geometry;
        }

        #endregion
    }

}
