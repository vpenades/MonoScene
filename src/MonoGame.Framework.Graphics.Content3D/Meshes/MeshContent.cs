using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    /// <summary>
    /// Represents the geometry (vertex+index buffers) of a <see cref="MeshPartContent"/>
    /// </summary>
    public class MeshGeometryContent
    {
        #region data        

        /// <summary>
        /// Logical Index into <see cref="MeshCollectionContent.SharedVertexBuffers"/>
        /// </summary>
        public int VertexBufferIndex { get; private set; }
        public int VertexOffset { get; private set; }
        public int VertexCount { get; private set; }        

        /// <summary>
        /// Logical index into <see cref="MeshCollectionContent.SharedIndexBuffers"/>
        /// </summary>
        public int IndexBufferIndex { get; private set; }
        public int IndexOffset { get; private set; }
        public int IndexCount { get; private set; }

        public int PrimitiveCount { get; private set; }

        // TODO: Morph targets would require a new DeviceBufferContent, and point to many of them.

        #endregion

        #region API

        public void SetVertices(int bufferIndex, int vertexOffset, int vertexCount)
        {
            VertexBufferIndex = bufferIndex;
            VertexOffset = vertexOffset;
            VertexCount = vertexCount;
        }

        public void SetIndices(int bufferIndex, int indexOffset, int indexCount, int primitiveCount)
        {
            IndexBufferIndex = bufferIndex;
            IndexOffset = indexOffset;
            IndexCount = indexCount;

            PrimitiveCount = primitiveCount;
        }

        #endregion
    }
    
    /// <summary>
    /// Represents a Mesh with a specific material, which is part of a <see cref="MeshContent"/>
    /// </summary>
    public class MeshPartContent
    {
        public MeshPartContent(MeshGeometryContent geometry, int materialIndex)
        {
            Geometry = geometry;
            MaterialIndex = materialIndex;
        }

        /// <summary>
        /// Logical Index into <see cref="MeshCollectionContent.SharedMaterials"/>
        /// </summary>
        public int MaterialIndex { get; private set; }
        public MeshGeometryContent Geometry { get; private set; }
    }

    /// <summary>
    /// Represents a Mesh, made of multiple <see cref="MeshPartContent"/>
    /// </summary>
    public class MeshContent
    {
        private readonly List<MeshPartContent> _Primitives = new List<MeshPartContent>();

        public IReadOnlyList<MeshPartContent> Parts => _Primitives;

        public void AddMeshPart(MeshGeometryContent geometry, int materialIndex)
        {
            var part = new MeshPartContent(geometry, materialIndex);
            _Primitives.Add(part);
        }
    }
}
