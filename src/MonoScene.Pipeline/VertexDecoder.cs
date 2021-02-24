using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNACOLOR = Microsoft.Xna.Framework.Color;

using PACKED = Microsoft.Xna.Framework.Graphics.PackedVector;


namespace MonoScene.Graphics.Pipeline
{
    static class VertexDecoder
    {
        #region API

        /// <summary>
        /// Gets the current Vertex attributes as an array of <see cref="{TVertex}"/> vertices.
        /// </summary>
        /// <typeparam name="TVertex">A Vertex type implementing <see cref="IVertexType"/>.</typeparam>
        /// <returns>A <see cref="{TVertex}"/> array</returns>
        public static TVertex[] ToXnaVertices<TVertex>(this IMeshPrimitiveDecoder srcMesh)
            where TVertex : unmanaged, IVertexType
        {
            var declaration = default(TVertex).VertexDeclaration;

            var vsize = Marshal.SizeOf<TVertex>();

            if (vsize != declaration.VertexStride) throw new ArgumentException(nameof(TVertex));

            var dst = new TVertex[srcMesh.VertexCount];

            PACKED.VertexInfluences skinning = default; // skin joints indices            

            for (int i = 0; i < dst.Length; ++i)
            {
                var dstv = _VertexWriter.CreateFromArray(dst, i);

                if (srcMesh.JointsWeightsCount > 0) skinning = srcMesh.GetSkinWeights(i);
                
                foreach (var element in declaration.GetVertexElements())
                {
                    switch (element.VertexElementUsage)
                    {
                        case VertexElementUsage.Position: dstv.SetValue(element, srcMesh.GetPosition(i)); break;
                        case VertexElementUsage.Normal: dstv.SetValue(element, srcMesh.GetNormal(i)); break;
                        case VertexElementUsage.Tangent: dstv.SetValue(element, srcMesh.GetTangent(i)); break;

                        case VertexElementUsage.TextureCoordinate: dstv.SetValue(element, srcMesh.GetTextureCoord(i, element.UsageIndex)); break;
                        case VertexElementUsage.Color: dstv.SetValue(element, srcMesh.GetColor(i, element.UsageIndex)); break;

                        case VertexElementUsage.BlendIndices: dstv.SetValue(element, skinning.Indices); break;
                        case VertexElementUsage.BlendWeight: dstv.SetValue(element, skinning.Weights); break;
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

                var d = MemoryMarshal.Cast<TVertex, Byte>(v);

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

            public void SetValue(VertexElement element, XNAV2 value)
            {
                if (element.VertexElementFormat == VertexElementFormat.Vector2)
                {
                    var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<XNAV2>());
                    MemoryMarshal.Write(dst, ref value);
                    return;
                }

                throw new NotImplementedException();
            }

            public void SetValue(VertexElement element, XNAV3 value)
            {
                if (element.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<XNAV3>());
                    MemoryMarshal.Write(dst, ref value);
                    return;
                }

                throw new NotImplementedException();
            }

            public void SetValue(VertexElement element, XNAV4 value)
            {
                var dst = _Vertex.Slice(element.Offset);

                switch (element.VertexElementFormat)
                {
                    case VertexElementFormat.Vector4:
                        MemoryMarshal.Write(dst, ref value);
                        return;

                    case VertexElementFormat.Color:
                        SetValue(element, new XNACOLOR(value));
                        return;

                    case VertexElementFormat.Byte4:
                        SetValue(element, new PACKED.Byte4(value));
                        return;

                    case VertexElementFormat.Short4:
                        SetValue(element, new PACKED.Short4(value));
                        return;

                    case VertexElementFormat.NormalizedShort4:
                        SetValue(element, new PACKED.NormalizedShort4(value));
                        return;
                }

                throw new NotImplementedException();
            }

            public void SetValue(VertexElement element, PACKED.Byte4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Byte4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<PACKED.Byte4>());
                MemoryMarshal.Write(dst, ref value);
            }

            public void SetValue(VertexElement element, XNACOLOR value)
            {
                if (element.VertexElementFormat != VertexElementFormat.Color) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<PACKED.Byte4>());
                MemoryMarshal.Write(dst, ref value);
            }

            public void SetValue(VertexElement element, PACKED.Short4 value)
            {
                if (element.VertexElementFormat == VertexElementFormat.Short4)
                {
                    var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<PACKED.Short4>());
                    MemoryMarshal.Write(dst, ref value);
                    return;
                }

                if (element.VertexElementFormat == VertexElementFormat.Byte4)
                {
                    var xval = new PACKED.Byte4(value.ToVector4());

                    var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf<PACKED.Byte4>());
                    MemoryMarshal.Write(dst, ref xval);
                    return;
                }

                throw new ArgumentException(nameof(element));
            }

            public void SetValue(VertexElement element, PACKED.NormalizedShort4 value)
            {
                if (element.VertexElementFormat != VertexElementFormat.NormalizedShort4) throw new ArgumentException(nameof(element));

                var dst = _Vertex.Slice(element.Offset, Marshal.SizeOf < PACKED.NormalizedShort4>());
                MemoryMarshal.Write(dst, ref value);
            }

            #endregion
        }

        #endregion  
    }
}
