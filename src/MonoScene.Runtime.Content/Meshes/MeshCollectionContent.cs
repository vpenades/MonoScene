using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics.Content
{
    /// <summary>
    /// Represents a collection of <see cref="MeshContent"/> objects and their associated resources.
    /// </summary>
    public class MeshCollectionContent
    {
        #region data        

        internal readonly List<VertexBufferContent> _SharedVertexBuffers = new List<VertexBufferContent>();
        internal readonly List<IndexBufferContent> _SharedIndexBuffers = new List<IndexBufferContent>();        

        internal readonly List<MeshContent> _Meshes = new List<MeshContent>();

        #endregion

        #region properties
        public IReadOnlyList<VertexBufferContent> SharedVertexBuffers => _SharedVertexBuffers;
        public IReadOnlyList<IndexBufferContent> SharedIndexBuffers => _SharedIndexBuffers;        
        public IReadOnlyList<MeshContent> Meshes => _Meshes;

        #endregion

        #region API

        public IEnumerable<(XNAV3 A, XNAV3 B, XNAV3 C)> EvaluateTriangles(int meshIndex, Func<XNAV3, VERTEXINFLUENCES,XNAV3> vertexTransform)
        {            
            var srcMesh = Meshes[meshIndex];            

            foreach (var srcPart in srcMesh.Parts)
            {
                var geo = srcPart.Geometry;

                var srcTriangles = _SharedIndexBuffers[geo.IndexBufferIndex]
                    .EvaluateTriangles(geo.IndexOffset,geo.IndexCount,geo.PrimitiveCount);

                var srcVertices = _SharedVertexBuffers[geo.VertexBufferIndex].GetEvaluator(geo.VertexOffset, geo.VertexCount);                

                foreach (var (idx0, idx1, idx2) in srcTriangles)
                {
                    var pos0 = srcVertices.GetPosition(idx0);
                    var pos1 = srcVertices.GetPosition(idx1);
                    var pos2 = srcVertices.GetPosition(idx2);

                    var sjw0 = srcVertices.GetBlend(idx0);
                    var sjw1 = srcVertices.GetBlend(idx1);
                    var sjw2 = srcVertices.GetBlend(idx2);

                    var a = vertexTransform(pos0, sjw0);
                    var b = vertexTransform(pos1, sjw1);
                    var c = vertexTransform(pos2, sjw2);

                    yield return (a, b, c);
                }
            }
            
        }

        #endregion
    }
}
