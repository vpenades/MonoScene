using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XY = System.Numerics.Vector2;
using XYZ = System.Numerics.Vector3;
using XYZW = System.Numerics.Vector4;

namespace SharpGLTF.Runtime.Content
{
    static class VertexDecoder
    {
        #region API

        /// <summary>
        /// Gets the current Vertex attributes as an array of <see cref="{TVertex}"/> vertices.
        /// </summary>
        /// <typeparam name="TVertex">A Vertex type implementing <see cref="IVertexType"/>.</typeparam>
        /// <returns>A <see cref="{TVertex}"/> array</returns>
        public static unsafe TVertex[] ToXnaVertices<TVertex>(this IMeshPrimitiveDecoder srcMesh)
            where TVertex : unmanaged, IVertexType
        {
            var declaration = default(TVertex).VertexDeclaration;

            if (sizeof(TVertex) != declaration.VertexStride) throw new ArgumentException(nameof(TVertex));

            var dst = new TVertex[srcMesh.VertexCount];

            XYZW jjjj = XYZW.Zero; // skin joints indices
            XYZW wwww = XYZW.Zero; // skin joints weights

            for (int i = 0; i < dst.Length; ++i)
            {
                var dstv = _VertexWriter.CreateFromArray(dst, i);

                if (srcMesh.JointsWeightsCount > 0)
                {
                    var jw = srcMesh.GetSkinWeights(i).GetReducedWeights(4); // ensures 4 weights
                    jjjj = new XYZW(jw.Index0, jw.Index1, jw.Index2, jw.Index3);
                    wwww = new XYZW(jw.Weight0, jw.Weight1, jw.Weight2, jw.Weight3);
                }
                
                foreach (var element in declaration.GetVertexElements())
                {
                    switch (element.VertexElementUsage)
                    {
                        case VertexElementUsage.Position: dstv.SetValue(element, srcMesh.GetPosition(i)); break;
                        case VertexElementUsage.Normal: dstv.SetValue(element, srcMesh.GetNormal(i)); break;
                        case VertexElementUsage.Tangent: dstv.SetValue(element, srcMesh.GetTangent(i)); break;

                        case VertexElementUsage.TextureCoordinate: dstv.SetValue(element, srcMesh.GetTextureCoord(i, element.UsageIndex)); break;
                        case VertexElementUsage.Color: dstv.SetValue(element, srcMesh.GetColor(i, element.UsageIndex)); break;

                        case VertexElementUsage.BlendIndices: dstv.SetValue(element, jjjj); break;
                        case VertexElementUsage.BlendWeight: dstv.SetValue(element, wwww); break;
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
                MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Color value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Color) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.Byte4));
                MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Microsoft.Xna.Framework.Graphics.PackedVector.Short4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Short4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.Short4));
                MemoryMarshal.Write(dst, ref value);
            }

            public unsafe void SetValue(VertexElement element, Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.NormalizedShort4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, sizeof(Microsoft.Xna.Framework.Graphics.PackedVector.NormalizedShort4));
                MemoryMarshal.Write(dst, ref value);
            }

            #endregion
        }

        #endregion  
    }
}
