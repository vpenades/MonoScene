using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Runtime.Graphics
{
    /// <summary>
    /// Represents a sequence of vertices, and can be used to create <see cref="VertexBuffer"/> resources.
    /// </summary>
    public class VertexBufferContent
    {
        #region Data

        private VertexElement[] _VertexElements;
        private int _VertexStride;        
        private int _VertexCount;
        private Byte[] _VertexData;

        #endregion

        #region properties

        public bool HasSkinning => _VertexElements.Any(item => item.VertexElementUsage == VertexElementUsage.BlendIndices);

        #endregion

        #region API

        public bool IsCompatibleWith<TVertex>()
            where TVertex:struct, IVertexType
        {
            return IsCompatibleWith(default(TVertex).VertexDeclaration);
        }

        public bool IsCompatibleWith(VertexDeclaration decl)
        {
            return new VertexDeclaration(_VertexStride, _VertexElements).Equals(decl);
        }

        public void Clear()
        {
            _VertexElements = null;
            _VertexStride = 0;
            _VertexCount = 0;
            _VertexData = null;
            
        }

        public (int Offset, int Count) AddVertices<T>(T[] vertices)
            where T : struct, IVertexType
        {
            var data = System.Runtime.InteropServices.MemoryMarshal.Cast<T, Byte>(vertices);
            return AddVertices(data, default(T).VertexDeclaration);
        }

        public (int Offset, int Count) AddVertices(ReadOnlySpan<Byte> vertexData, VertexDeclaration vertexFormat)
        {
            var count = vertexData.Length / vertexFormat.VertexStride;

            if (count == 0) return (_VertexCount, 0);            

            if (_VertexCount == 0)
            {
                _VertexElements = vertexFormat.GetVertexElements();
                _VertexStride = vertexFormat.VertexStride;
            }
            else
            {
                var thisVD = new VertexDeclaration(_VertexStride, _VertexElements);
                if (!thisVD.Equals(vertexFormat)) throw new ArgumentException(nameof(vertexFormat));                
            }

            vertexData = vertexData.Slice(0, count * _VertexStride);

            int offset = _VertexCount;

            Array.Resize(ref _VertexData, (offset + count) * _VertexStride);
            vertexData.CopyTo(_VertexData.AsSpan().Slice(offset * _VertexStride));

            _VertexCount += count;

            return (offset, count);
        }

        public VertexBuffer CreateVertexBuffer(GraphicsDevice graphics)
        {
            // https://github.com/MonoGame/MonoGame/blob/655b4f05c4aeb50fe416c2dc92a9afcf294e2fd8/MonoGame.Framework/Content/ContentReaders/VertexBufferReader.cs#L12

            var vdecl = new VertexDeclaration(_VertexStride, _VertexElements);
            var vb = new VertexBuffer(graphics, vdecl, _VertexCount, BufferUsage.None);            

            vb.SetData(_VertexData, 0, _VertexCount * vdecl.VertexStride );            

            return vb;
        }

        #endregion
    }
}
