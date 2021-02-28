using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAQ = Microsoft.Xna.Framework.Quaternion;
using XNA4X4 = Microsoft.Xna.Framework.Matrix;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    static class Converters
    {
        public static XNAV2 ToXna(this Assimp.Vector2D v) { return new XNAV2(v.X, v.Y); }

        public static XNAV3 ToXna(this Assimp.Vector3D v) { return new XNAV3(v.X, v.Y, v.Z); }

        public static XNAV3 ToXnaVector(this Assimp.Color3D v) { return new XNAV3(v.R, v.G, v.B); }
        public static XNAV4 ToXnaVector(this Assimp.Color4D v) { return new XNAV4(v.R, v.G, v.B, v.A); }

        public static XNAQ ToXna(this Assimp.Quaternion q) { return new XNAQ(q.X, q.Y, q.Z, q.W); }        

        public static XNA4X4 ToXna(this Assimp.Matrix4x4 m)
        {
            // Assimp matrix needs to be transposed relative to Xna Matrix.

            return new XNA4X4
                (m.A1, m.B1, m.C1, m.D1
                , m.A2, m.B2, m.C2, m.D2
                , m.A3, m.B3, m.C3, m.D3
                , m.A4, m.B4, m.C4, m.D4);
        }

        public static (XNAV3 U, XNAV3 V) ToXna(this System.Numerics.Matrix3x2 m)
        {
            var u = new XNAV3(m.M11, m.M21, m.M31);
            var v = new XNAV3(m.M12, m.M22, m.M32);

            return (u, v);
        }

        

        public static IReadOnlyList<IMeshDecoder<int>> ToXna(this IReadOnlyList<Assimp.Mesh> srcMeshes, AssimpMaterialsFactory srcMaterials)
        {
            var dstMeshes = srcMeshes
                .Select(item => new _MeshDecoder<int>(item, item.MaterialIndex))
                .Cast<IMeshDecoder<int>>()
                .ToArray();

            return dstMeshes;
        }

        
    }
}
