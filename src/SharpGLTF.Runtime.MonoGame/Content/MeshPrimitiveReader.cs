using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Schema2;

using XY = System.Numerics.Vector2;
using XYZ = System.Numerics.Vector3;
using XYZW = System.Numerics.Vector4;

namespace SharpGLTF.Runtime.Content
{
    /// <summary>
    /// Reads the content of a glTF <see cref="MeshPrimitive"/> object into a structure that's easier to consume by MonoGame.
    /// </summary>
    public sealed class MeshPrimitiveReader
    {
        #region lifecycle

        internal MeshPrimitiveReader(MeshPrimitive srcPrim)
        {
            // the first geometry block is the base mesh.
            var baseGeometry = new MeshGeometryReader(this, srcPrim);
            _Geometries.Add(baseGeometry);

            // additional geometry blocks are the morph targets (if any)
            for(int i=0; i < srcPrim.MorphTargetsCount; ++i)
            {
                var morphTarget = new MeshGeometryReader(this, srcPrim, i);
                _Geometries.Add(morphTarget);
            }
            
            _Color0 = srcPrim.GetVertexAccessor("COLOR_0")?.AsColorArray();
            _TexCoord0 = srcPrim.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();
            _TexCoord1 = srcPrim.GetVertexAccessor("TEXCOORD_1")?.AsVector2Array();

            _Joints0 = srcPrim.GetVertexAccessor("JOINTS_0")?.AsVector4Array();
            _Joints1 = srcPrim.GetVertexAccessor("JOINTS_1")?.AsVector4Array();
            _Weights0 = srcPrim.GetVertexAccessor("WEIGHTS_0")?.AsVector4Array();
            _Weights1 = srcPrim.GetVertexAccessor("WEIGHTS_1")?.AsVector4Array();

            if (_Joints0 == null || _Weights0 == null) { _Joints0 = _Joints1 = _Weights0 = _Weights1 = null; }
            if (_Joints1 == null || _Weights1 == null) { _Joints1 = _Weights1 = null; }

            if (_Weights0 != null)
            {
                _Weights0 = _Weights0.ToArray(); // isolate memory to prevent overwriting source glTF.

                for (int i = 0; i < _Weights0.Count; ++i)
                {
                    var r = XYZW.Dot(_Weights0[i], XYZW.One);
                    _Weights0[i] /= r;
                }
            }

            _TrianglesSource = srcPrim.GetTriangleIndices().ToArray();

            _Triangles = _TrianglesSource;
        }

        #endregion

        #region data

        private readonly (int A, int B, int C)[] _TrianglesSource;

        private readonly (int A, int B, int C)[] _Triangles;

        private readonly List<MeshGeometryReader> _Geometries = new List<MeshGeometryReader>();        

        private readonly IList<XYZW> _Color0;
        private readonly IList<XY> _TexCoord0;
        private readonly IList<XY> _TexCoord1;

        private readonly IList<XYZW> _Joints0;
        private readonly IList<XYZW> _Joints1;

        private readonly IList<XYZW> _Weights0;
        private readonly IList<XYZW> _Weights1;

        #endregion

        #region properties        

        public bool IsSkinned => _Joints0 != null;

        public bool hasMorphTargets => _Geometries.Count > 1;

        public int VertexCount => _Geometries[0].VertexCount;

        public (int A, int B, int C)[] TriangleIndices => _Triangles;

        internal IReadOnlyList<MeshGeometryReader> Geometries => _Geometries;

        public BoundingSphere BoundingSphere
        {
            get
            {
                var points = _Geometries[0]._Positions.Select(item => item.ToXna());
                return BoundingSphere.CreateFromPoints(points);
            }
        }

        #endregion

        #region API

        public static void GenerateNormalsAndTangents(IEnumerable<MeshPrimitiveReader> srcPrims)
        {
            if (!srcPrims.Any()) return;

            // find out the number of morph targets (index 0 is base mesh)
            var morphTargetsCount = srcPrims.Min(item => item.Geometries.Count);

            // generate normals and tangents
            for (int i = 0; i < morphTargetsCount; ++i)
            {
                var morphTargets = srcPrims.Select(item => item.Geometries[i]).ToList();
                VertexNormalsFactory.CalculateSmoothNormals(morphTargets);
                VertexTangentsFactory.CalculateTangents(morphTargets);
            }
        }

        public XYZ GetPosition(int idx) { return _Geometries[0].GetPosition(idx); }

        public XYZ GetNormal(int idx) { return _Geometries[0].GetNormal(idx); }

        public XYZW GetTangent(int idx) { return _Geometries[0].GetTangent(idx); }

        public XY GetTextureCoord(int idx, int set)
        {
            if (set == 0 && _TexCoord0 != null) return _TexCoord0[idx];
            if (set == 1 && _TexCoord1 != null) return _TexCoord1[idx];

            return XY.Zero;
        }

        public XYZW GetColor(int idx, int set)
        {
            if (set == 0 && _Color0 != null) return _Color0[idx];

            return XYZW.One;
        }

        public XYZW GetIndices(int idx)
        {
            if (_Joints0 != null) return _Joints0[idx];
            return XYZW.Zero;
        }

        public XYZW GetWeights(int idx)
        {
            if (_Weights0 != null) return _Weights0[idx];
            return XYZW.UnitX;
        }

        /// <summary>
        /// Gets the current Vertex attributes as an array of <see cref="{TVertex}"/> vertices.
        /// </summary>
        /// <typeparam name="TVertex">A Vertex type implementing <see cref="IVertexType"/>.</typeparam>
        /// <returns>A <see cref="{TVertex}"/> array</returns>
        public unsafe TVertex[] ToXnaVertices<TVertex>()
            where TVertex:unmanaged, IVertexType
        {
            var declaration = default(TVertex).VertexDeclaration;

            if (sizeof(TVertex) != declaration.VertexStride) throw new ArgumentException(nameof(TVertex));

            var dst = new TVertex[VertexCount];

            for (int i = 0; i < dst.Length; ++i)
            {
                var v = _VertexWriter.CreateFromArray(dst, i);                

                foreach(var element in declaration.GetVertexElements())
                {
                    switch(element.VertexElementUsage)
                    {
                        case VertexElementUsage.Position: v.SetValue(element, GetPosition(i)); break;
                        case VertexElementUsage.Normal: v.SetValue(element, GetNormal(i)); break;
                        case VertexElementUsage.Tangent: v.SetValue(element, GetTangent(i)); break;

                        case VertexElementUsage.TextureCoordinate: v.SetValue(element, GetTextureCoord(i, element.UsageIndex)); break;
                        case VertexElementUsage.Color: v.SetValue(element, GetColor(i, element.UsageIndex)); break;

                        case VertexElementUsage.BlendIndices: v.SetValue(element, GetIndices(i)); break;
                        case VertexElementUsage.BlendWeight: v.SetValue(element, GetWeights(i)); break;
                    }                            
                }                
            }

            return dst;
        }

        public unsafe TVertex[] ToXnaMorphTargets<TVertex>()
            where TVertex : unmanaged, IVertexType
        {
            var declaration = default(TVertex).VertexDeclaration;

            if (sizeof(TVertex) != declaration.VertexStride) throw new ArgumentException(nameof(TVertex));

            var dst = new TVertex[VertexCount * _Geometries.Count];

            for (int i = 0; i < dst.Length; ++i)
            {
                var v = _VertexWriter.CreateFromArray(dst, i);

                foreach (var element in declaration.GetVertexElements())
                {
                    var geometry = _Geometries[i / VertexCount];

                    var ii = i % VertexCount;

                    switch (element.VertexElementUsage)
                    {
                        case VertexElementUsage.Position: v.SetValue(element, geometry.GetPosition(ii)); break;
                        case VertexElementUsage.Normal: v.SetValue(element, geometry.GetNormal(ii)); break;
                        case VertexElementUsage.Tangent: v.SetValue(element, geometry.GetTangent(ii)); break;                        
                    }
                }
            }

            return dst;
        }

        #endregion

        #region nested types
        readonly ref struct _VertexWriter 
        {
            #region constructor
            public static _VertexWriter CreateFromArray<TVertex>(TVertex[] vvv, int idx)
                where TVertex : unmanaged, IVertexType
            {
                var v = vvv.AsSpan().Slice(idx, 1);

                var d = System.Runtime.InteropServices.MemoryMarshal.Cast<TVertex, Byte>(v);

                return new _VertexWriter(d);
            }

            public _VertexWriter(Span<Byte> vertex)
            {
                _Vertex = vertex;
            }

            #endregion

            #region data

            private readonly Span<Byte> _Vertex;

            #endregion

            #region API            

            public unsafe void SetValue(VertexElement element, XY value)
            {
                if (element.VertexElementFormat == VertexElementFormat.Vector2)
                {
                    var dst = _Vertex.Slice(element.Offset, sizeof(XY));
                    System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
                    return;
                }

                throw new NotImplementedException();
            }

            public unsafe void SetValue(VertexElement element, XYZ value)
            {
                if (element.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    var dst = _Vertex.Slice(element.Offset, sizeof(XYZ));
                    System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
                    return;
                }

                throw new NotImplementedException();
            }

            public unsafe void SetValue(VertexElement element, XYZW value)
            {
                var dst = _Vertex.Slice(element.Offset);

                switch (element.VertexElementFormat)
                {
                    case VertexElementFormat.Vector4:                        
                        System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
                        return;

                    case VertexElementFormat.Color:                        
                        SetValue(element, new Color(value.ToXna()));
                        return;

                    case VertexElementFormat.Byte4:
                        SetValue(element, new Microsoft.Xna.Framework.Graphics.PackedVector.Byte4(value.ToXna()));                        
                        return;
                    
                    case VertexElementFormat.Short4:
                        SetValue(element, new Microsoft.Xna.Framework.Graphics.PackedVector.Short4(value.ToXna()));
                        return;

                    case VertexElementFormat.NormalizedShort4:
                        SetValue(element, new Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4(value.ToXna()));
                        return;
                }

                throw new NotImplementedException();
            }

            public unsafe void SetValue(VertexElement element, Microsoft.Xna.Framework.Graphics.PackedVector.Byte4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Byte4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.Byte4));
                System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Color value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Color) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.Byte4));
                System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Microsoft.Xna.Framework.Graphics.PackedVector.Short4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Short4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.Short4));
                System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.NormalizedShort4) throw new ArgumentException(nameof(element));
                
                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4));
                System.Runtime.InteropServices.MemoryMarshal.Write(dst, ref value);                
            }

            #endregion
        }

        #endregion        
    }

    [System.Diagnostics.DebuggerDisplay("Vertices: {VertexCount}")]
    class MeshGeometryReader
        : VertexNormalsFactory.IMeshPrimitive
        , VertexTangentsFactory.IMeshPrimitive
    {
        #region  lifecycle

        public MeshGeometryReader(MeshPrimitiveReader owner, MeshPrimitive srcPrim)
        {
            _Owner = owner;

            _Positions = srcPrim.GetVertexAccessor("POSITION")?.AsVector3Array();
            _Normals = srcPrim.GetVertexAccessor("NORMAL")?.AsVector3Array();
            _Tangents = srcPrim.GetVertexAccessor("TANGENT")?.AsVector4Array();
        }

        public MeshGeometryReader(MeshPrimitiveReader owner, MeshPrimitive srcPrim, int morphTargetIndex)            
        {
            _Owner = owner;

            // copy the Base Geometry from Geometry 0

            _Positions = _Owner.Geometries[0]?._Positions?.ToArray();
            _Normals = _Owner.Geometries[0]?._Normals?.ToArray();
            _Tangents = _Owner.Geometries[0]?._Tangents?.ToArray();

            // get morph deltas and apply them to our base geometry copy.

            var morphs = srcPrim.GetMorphTargetAccessors(morphTargetIndex);

            if (morphs.TryGetValue("POSITION", out Accessor pAccessor))
            {
                var pDeltas = pAccessor.AsVector3Array();
                for (int i = 0; i < _Positions.Count; ++i)
                {
                    _Positions[i] += pDeltas[i];
                }
            }

            if (morphs.TryGetValue("NORMAL", out Accessor nAccessor))
            {
                var nDeltas = nAccessor.AsVector3Array();
                for (int i = 0; i < _Positions.Count; ++i)
                {
                    _Normals[i] += nDeltas[i];
                }
            }

            if (morphs.TryGetValue("TANGENT", out Accessor tAccessor))
            {
                var tDeltas = tAccessor.AsVector3Array();
                for (int i = 0; i < _Positions.Count; ++i)
                {
                    _Tangents[i] += new XYZW(tDeltas[i], 0);
                }
            }
        }

        #endregion

        #region data

        private readonly MeshPrimitiveReader _Owner;

        internal readonly IList<XYZ> _Positions;
        private IList<XYZ> _Normals;
        private IList<XYZW> _Tangents;

        #endregion

        #region properties

        public int VertexCount => _Positions?.Count ?? 0;

        #endregion

        #region API

        public XYZ GetPosition(int idx) { return _Positions[idx]; }

        public XYZ GetNormal(int idx) { return _Normals[idx]; }

        public XYZW GetTangent(int idx) { return _Tangents[idx]; }

        public XY GetTextureCoord(int idx, int set) { return _Owner.GetTextureCoord(idx, set); }

        #endregion

        #region Support methods for VertexNormalsFactory and VertexTangentsFactory

        IEnumerable<(int A, int B, int C)> VertexNormalsFactory.IMeshPrimitive.GetTriangleIndices() { return _Owner.TriangleIndices; }

        IEnumerable<(int A, int B, int C)> VertexTangentsFactory.IMeshPrimitive.GetTriangleIndices() { return _Owner.TriangleIndices; }

        XYZ VertexNormalsFactory.IMeshPrimitive.GetVertexPosition(int idx) { return GetPosition(idx); }
        XYZ VertexTangentsFactory.IMeshPrimitive.GetVertexPosition(int idx) { return GetPosition(idx); }
        XYZ VertexTangentsFactory.IMeshPrimitive.GetVertexNormal(int idx) { return GetNormal(idx); }
        XY VertexTangentsFactory.IMeshPrimitive.GetVertexTexCoord(int idx) { return GetTextureCoord(idx, 0); }

        void VertexNormalsFactory.IMeshPrimitive.SetVertexNormal(int idx, XYZ normal)
        {
            if (_Normals == null) _Normals = new XYZ[VertexCount];
            if (!(_Normals is XYZ[])) return; // if it's not a plain array, it's a glTF source, so we prevent writing existing normals.            
            _Normals[idx] = normal;
        }

        void VertexTangentsFactory.IMeshPrimitive.SetVertexTangent(int idx, XYZW tangent)
        {
            if (_Tangents == null) _Tangents = new XYZW[VertexCount];
            if (!(_Tangents is XYZW[])) return; // if it's not a plain array, it's a glTF source, so we prevent writing existing tangents.            
            _Tangents[idx] = tangent;
        }

        #endregion
    }

    sealed class MeshPrimitiveWriter
    {
        #region data

        // shared buffers
        private readonly Dictionary<Type, IPrimitivesBuffers> _Buffers = new Dictionary<Type, IPrimitivesBuffers>();

        // primitives
        private readonly List<_MeshPrimitive> _MeshPrimitives = new List<_MeshPrimitive>();

        #endregion

        #region API

        public void WriteMeshPrimitive<TVertex>(int logicalMeshIndex, Effect effect,BlendState blending, bool doubleSided, MeshPrimitiveReader primitive)
            where TVertex : unmanaged, IVertexType
        {
            if (!_Buffers.TryGetValue(typeof(TVertex), out IPrimitivesBuffers pb))
            {
                _Buffers[typeof(TVertex)] = pb = new _PrimitivesBuffers<TVertex>();
            }

            var part = (pb as _PrimitivesBuffers<TVertex>).Append(logicalMeshIndex, effect, blending, doubleSided, primitive);

            _MeshPrimitives.Add(part);
        }

        internal IReadOnlyDictionary<int, RuntimeModelMesh> GetRuntimeMeshes(GraphicsDevice device, GraphicsResourceTracker disposables)
        {
            // create shared vertex/index buffers

            var vbuffers = _Buffers.Values.ToDictionary(key => key, val => val.CreateVertexBuffer(device));
            var ibuffers = _Buffers.Values.ToDictionary(key => key, val => val.CreateIndexBuffer(device));

            foreach (var vb in vbuffers.Values) disposables.AddDisposable(vb);
            foreach (var ib in ibuffers.Values) disposables.AddDisposable(ib);

            // create RuntimeModelMesh

            RuntimeModelMesh _convert(IEnumerable<_MeshPrimitive> srcParts)
            {
                var dstMesh = new RuntimeModelMesh(device);

                foreach(var srcPart in srcParts)
                {
                    var vb = vbuffers[srcPart.PrimitiveBuffers];
                    var ib = ibuffers[srcPart.PrimitiveBuffers];

                    var dstPart = dstMesh.CreateMeshPart();
                    dstPart.Effect = srcPart.Material.PrimitiveEffect;
                    dstPart.Blending = srcPart.Material.PrimitiveBlending;
                    dstPart.FrontRasterizer = srcPart.Material.DoubleSided ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise;
                    dstPart.BackRasterizer = srcPart.Material.DoubleSided ? RasterizerState.CullNone : RasterizerState.CullClockwise;
                    dstPart.BoundingSphere = srcPart.BoundingSphere;
                    dstPart.SetVertexBuffer(vb, srcPart.VertexOffset, srcPart.VertexCount);
                    dstPart.SetIndexBuffer(ib, srcPart.TriangleOffset * 3, srcPart.TriangleCount);                    
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
            private readonly List<(int,int,int)> _Triangles = new List<(int, int, int)>();

            #endregion

            #region API

            public _MeshPrimitive Append(int meshKey, Effect effect, BlendState blending, bool doubleSided, MeshPrimitiveReader primitive)
            {
                var partVertices = primitive.ToXnaVertices<TVertex>();
                var partTriangles = primitive.TriangleIndices;

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
                    TriangleCount = partTriangles.Length,
                    BoundingSphere = primitive.BoundingSphere

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

            public BoundingSphere BoundingSphere;
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
