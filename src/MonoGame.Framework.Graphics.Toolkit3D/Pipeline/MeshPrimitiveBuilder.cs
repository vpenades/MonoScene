using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Accumulates all the primitives of all the meshes of a model, so they can be optimized/batched.
    /// </summary>
    sealed class MeshPrimitiveBuilder
    {
        #region data

        // shared buffers
        private readonly Dictionary<Type, IPrimitivesBuffers> _Buffers = new Dictionary<Type, IPrimitivesBuffers>();

        // primitives
        private readonly List<_MeshPrimitive> _MeshPrimitives = new List<_MeshPrimitive>();

        #endregion

        #region API

        public void AppendMeshPrimitive(int logicalMeshIndex, Type vertexType, IMeshPrimitiveDecoder primitive, Effect effect, BlendState blending, bool doubleSided)
        {
            // this is a reflection hack to call a generic method from a non generic method.

            var myMethod = typeof(MeshPrimitiveBuilder)
              .GetMethods()
              .FirstOrDefault(m => m.Name == nameof(AppendMeshPrimitive) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

            myMethod = myMethod.MakeGenericMethod(vertexType);
            myMethod.Invoke(this, new Object[] { logicalMeshIndex, primitive, effect, blending, doubleSided });
        }

        public void AppendMeshPrimitive<TVertex>(int logicalMeshIndex, IMeshPrimitiveDecoder primitive, Effect effect, BlendState blending, bool doubleSided)
            where TVertex : unmanaged, IVertexType
        {
            if (!_Buffers.TryGetValue(typeof(TVertex), out IPrimitivesBuffers pb))
            {
                _Buffers[typeof(TVertex)] = pb = new _PrimitivesBuffers<TVertex>();
            }

            var part = (pb as _PrimitivesBuffers<TVertex>).Append(logicalMeshIndex, effect, blending, doubleSided, primitive);

            _MeshPrimitives.Add(part);
        }

        internal IReadOnlyDictionary<int, Mesh> CreateRuntimeMeshes(GraphicsDevice device, GraphicsResourceTracker disposables)
        {
            // create shared vertex/index buffers

            var vbuffers = _Buffers.Values.ToDictionary(key => key, val => val.CreateVertexBuffer(device));
            var ibuffers = _Buffers.Values.ToDictionary(key => key, val => val.CreateIndexBuffer(device));
            
            foreach (var vb in vbuffers.Values) disposables.AddDisposable(vb);
            foreach (var ib in ibuffers.Values) disposables.AddDisposable(ib);            

            // create RuntimeModelMesh

            Mesh _convert(IEnumerable<_MeshPrimitive> srcParts)
            {
                var dstMesh = new Mesh(device);

                foreach (var srcPart in srcParts)
                {
                    var vb = vbuffers[srcPart.PrimitiveBuffers];
                    var ib = ibuffers[srcPart.PrimitiveBuffers];                    

                    var dstGeo = new MeshTriangles();
                    dstGeo.SetCullingStates(srcPart.Material.DoubleSided);                    
                    dstGeo.SetVertexBuffer(vb, srcPart.VertexOffset, srcPart.VertexCount);
                    dstGeo.SetIndexBuffer(ib, srcPart.TriangleOffset * 3, srcPart.TriangleCount);

                    var dstPart = dstMesh.CreateMeshPart();
                    dstPart.Effect = srcPart.Material.PrimitiveEffect;
                    dstPart.Blending = srcPart.Material.PrimitiveBlending;
                    dstPart.Geometry = dstGeo;
                }

                return dstMesh;
            }

            return _MeshPrimitives
                .GroupBy(item => item.LogicalMeshIndex)
                .ToDictionary(k => k.Key, v => _convert(v));
        }

        #endregion

        #region nested types

        interface IPrimitivesBuffers
        {
            VertexBuffer CreateVertexBuffer(GraphicsDevice device);
            IndexBuffer CreateIndexBuffer(GraphicsDevice device);
        }

        /// <summary>
        /// Contains the shared vertex/index buffers of all the mesh primitive that share the same vertex type.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        sealed class _PrimitivesBuffers<TVertex> : IPrimitivesBuffers
            where TVertex : unmanaged, IVertexType
        {
            #region data

            private readonly List<TVertex> _Vertices = new List<TVertex>();
            private readonly List<(int, int, int)> _Triangles = new List<(int, int, int)>();

            #endregion

            #region API
            public _MeshPrimitive Append(int meshKey, Effect effect, BlendState blending, bool doubleSided, IMeshPrimitiveDecoder primitive)
            {
                var partVertices = primitive.ToXnaVertices<TVertex>();
                var partTriangles = primitive.TriangleIndices.ToList();

                var material = new _Material
                {
                    PrimitiveEffect = effect,
                    PrimitiveBlending = blending,
                    DoubleSided = doubleSided
                };

                var part = new _MeshPrimitive
                {
                    LogicalMeshIndex = meshKey,
                    Material = material,
                    PrimitiveBuffers = this,
                    VertexOffset = _Vertices.Count,
                    VertexCount = partVertices.Length,
                    TriangleOffset = _Triangles.Count,
                    TriangleCount = partTriangles.Count
                };

                _Vertices.AddRange(partVertices);
                _Triangles.AddRange(partTriangles);

                return part;
            }

            public VertexBuffer CreateVertexBuffer(GraphicsDevice device)
            {
                var data = new VertexBuffer(device, typeof(TVertex), _Vertices.Count, BufferUsage.None);
                data.SetData(_Vertices.ToArray());
                return data;
            }

            public IndexBuffer CreateIndexBuffer(GraphicsDevice device)
            {
                return CreateIndexBuffer(device, _Triangles);
            }

            private static IndexBuffer CreateIndexBuffer(GraphicsDevice device, IEnumerable<(int A, int B, int C)> triangles)
            {
                var sequence32 = triangles
                    .SelectMany(item => new[] { (UInt32)item.C, (UInt32)item.B, (UInt32)item.A })
                    .ToArray();

                var max = sequence32.Max();

                if (max > 65535)
                {
                    var indices = new IndexBuffer(device, typeof(UInt32), sequence32.Length, BufferUsage.None);

                    indices.SetData(sequence32);
                    return indices;
                }
                else
                {
                    var sequence16 = sequence32.Select(item => (UInt16)item).ToArray();

                    var indices = new IndexBuffer(device, typeof(UInt16), sequence16.Length, BufferUsage.None);

                    indices.SetData(sequence16);
                    return indices;
                }
            }

            #endregion
        }

        /// <summary>
        /// Represents a mesh primitive
        /// </summary>
        struct _MeshPrimitive
        {
            public int LogicalMeshIndex;
            public _Material Material;
            public IPrimitivesBuffers PrimitiveBuffers;
            public int VertexOffset;
            public int VertexCount;
            public int TriangleOffset;
            public int TriangleCount;            
        }

        struct _Material
        {
            public Effect PrimitiveEffect;
            public BlendState PrimitiveBlending;
            public bool DoubleSided;
        }

        #endregion
    }
}
