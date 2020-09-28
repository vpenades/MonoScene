using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    [System.Diagnostics.DebuggerDisplay("{_ToDebugString(),nq}")]
    struct VertexRigid : IVertexType
    {
        #region debug

        private string _ToDebugString()
        {
            var p = $"{Position.X:N5} {Position.Y:N5} {Position.Z:N5}";
            var n = $"{Normal.X:N2} {Normal.Y:N2} {Normal.Z:N2}";
            var t = $"{Tangent.X:N2} {Tangent.Y:N2} {Tangent.Z:N2} {Tangent.W:N1}";
            var uv0 = $"{TextureCoordinate0.X:N3} {TextureCoordinate0.Y:N3}";
            var uv1 = $"{TextureCoordinate1.X:N3} {TextureCoordinate1.Y:N3}";

            return $"𝐏:{p}   𝚴:{n}   𝚻:{t}   𝐂:{Color.PackedValue:X}   𝐔𝐕₀:{uv0}   𝐔𝐕₁:{uv1}";
        }

        #endregion

        #region static

        private static VertexDeclaration _VDecl = CreateVertexDeclaration();

        public static VertexDeclaration CreateVertexDeclaration()
        {
            int offset = 0;

            var a = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
            offset += 3 * 4;

            var b = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
            offset += 3 * 4;

            var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
            offset += 4 * 4;

            var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
            offset += 4 * 1;

            var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
            offset += 2 * 4;

            var f = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1);
            offset += 2 * 4;

            return new VertexDeclaration(a, b, c, d, e, f);
        }

        #endregion

        #region data

        public VertexDeclaration VertexDeclaration => _VDecl;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Color Color;
        public Vector2 TextureCoordinate0;
        public Vector2 TextureCoordinate1;

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("{_ToDebugString(),nq}")]
    struct VertexSkinned : IVertexType
    {
        #region debug

        private string _ToDebugString()
        {
            var p = $"{Position.X:N5} {Position.Y:N5} {Position.Z:N5}";
            var n = $"{Normal.X:N2} {Normal.Y:N2} {Normal.Z:N2}";
            var t = $"{Tangent.X:N2} {Tangent.Y:N2} {Tangent.Z:N2} {Tangent.W:N1}";
            var uv0 = $"{TextureCoordinate0.X:N3} {TextureCoordinate0.Y:N3}";
            var uv1 = $"{TextureCoordinate1.X:N3} {TextureCoordinate1.Y:N3}";
            var jv = BlendIndices.ToVector4();
            var j = $"{jv.X:N3} {jv.Y:N3} {jv.Z:N3} {jv.W:N3}";

            return $"𝐏:{p}   𝚴:{n}   𝚻:{t}   𝐂:{Color.PackedValue:X}   𝐔𝐕₀:{uv0}   𝐔𝐕₁:{uv1}   𝐉𝐖:{j}";
        }

        #endregion

        #region static

        private static VertexDeclaration _VDecl = CreateVertexDeclaration();

        public static VertexDeclaration CreateVertexDeclaration()
        {
            int offset = 0;

            var a = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
            offset += 3 * 4;

            var b = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
            offset += 3 * 4;

            var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
            offset += 4 * 4;

            var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
            offset += 4 * 1;

            var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
            offset += 2 * 4;

            var f = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1);
            offset += 2 * 4;

            var g = new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0);
            offset += 4 * 1;

            var h = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0);
            offset += 4 * 4;

            return new VertexDeclaration(a, b, c, d, e, f, g, h);
        }

        #endregion

        #region data

        public VertexDeclaration VertexDeclaration => _VDecl;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Color Color;
        public Vector2 TextureCoordinate0;
        public Vector2 TextureCoordinate1;
        public PackedVector.Byte4 BlendIndices;
        public Vector4 BlendWeight;

        #endregion
    }    
}
