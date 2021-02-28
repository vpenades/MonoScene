using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Helper class used to coalesce mesh resources and build a <see cref="MeshCollectionContent"/>
    /// </summary>
    public class MeshCollectionBuilder
    {
        #region lifecycle

        public MeshCollectionBuilder() { }

        public MeshCollectionBuilder(IReadOnlyList<IMeshDecoder<int>> srcMeshes)
        {
            for (int i = 0; i < srcMeshes.Count; ++i)
            {
                var srcMesh = srcMeshes[i];
                var hasSkin = srcMesh.Primitives.Any(item => item.JointsWeightsCount > 0);

                this.AppendMesh(srcMesh);
            }
        }

        #endregion

        #region data        

        // shared buffers
        internal readonly List<VertexBufferContent> _VertexBuffers = new List<VertexBufferContent>();
        internal readonly List<IndexBufferContent> _IndexBuffers = new List<IndexBufferContent>();        
        internal readonly List<MeshContent> _Meshes = new List<MeshContent>();

        #endregion

        #region API

        public static MeshCollectionContent CreateContent(IReadOnlyList<IMeshDecoder<int>> srcMeshes)
        {
            return new MeshCollectionBuilder(srcMeshes).ToContent();
        }

        public MeshCollectionContent ToContent()
        {
            var dstMeshes = new MeshCollectionContent();

            dstMeshes._SharedVertexBuffers.AddRange(this._VertexBuffers);
            dstMeshes._SharedIndexBuffers.AddRange(this._IndexBuffers);            
            dstMeshes._Meshes.AddRange(this._Meshes);

            return dstMeshes;
        }

        #endregion

        #region core

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
            return _IndexBuffers.Count - 1;
        }

        public void AppendMesh(IMeshDecoder<int> srcMesh)
        {
            var dstMesh = new MeshContent();
            dstMesh.Name = srcMesh.Name;
            dstMesh.Tag = srcMesh.Tag;

            foreach (var prim in srcMesh.Primitives)
            {
                var vdecl = MeshPrimitiveDecoder.GetVertexDeclaration(prim);

                int vbIndex = _UseVertexBuffer(vdecl);
                int ibIndex = _UseIndexBuffer();

                var geometry = CreateGeometry(vbIndex, ibIndex, prim, vdecl);
                if (geometry == null) continue;

                dstMesh.AddMeshPart(geometry, prim.Material);
            }

            _Meshes.Add(dstMesh);
        }

        private MeshGeometryContent CreateGeometry(int vbIndex, int ibIndex, IMeshPrimitiveDecoder<int> primitive, VertexDeclaration vdecl)
        {
            var partVertices = MeshPrimitiveDecoder.ToXnaVertices(primitive, vdecl);
            var partTriangles = primitive.TriangleIndices.ToList();

            if (partTriangles.Count == 0) return null;

            var vRange = _VertexBuffers[vbIndex].AddVertices(partVertices, vdecl);
            var iRange = _IndexBuffers[ibIndex].AddTriangleIndices(partTriangles);

            var geometry = new MeshGeometryContent();
            geometry.SetVertices(vbIndex, vRange.VertexOffset, vRange.VertexCount);
            geometry.SetIndices(ibIndex, iRange.Offset, iRange.Count, partTriangles.Count);

            return geometry;
        }

        #endregion
    }
}
