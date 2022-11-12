using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;


namespace MonoScene.Graphics.Pipeline
{
    readonly struct _MeshDecoder : IMeshDecoder<int>
    {
        #region constructor
        public _MeshDecoder(SharpGLTF.Runtime.IMeshDecoder<SharpGLTF.Schema2.Material> mesh, GLTFMaterialsFactory materials, object tag)
        {
            _Source = mesh;
            _Primitives = mesh.Primitives.Select(item => _MeshPrimitiveDecoder.Create(item, materials)).ToArray();
            _Tag = tag;
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshDecoder<SharpGLTF.Schema2.Material> _Source;
        private readonly IMeshPrimitiveDecoder<int>[] _Primitives;
        private readonly object _Tag;

        #endregion

        #region properties

        public string Name => _Source.Name;
        public object Tag => _Tag;

        public IReadOnlyList<IMeshPrimitiveDecoder<int>> Primitives => _Primitives;        

        #endregion
    }

    readonly struct _MeshPrimitiveDecoder : IMeshPrimitiveDecoder<int>
    {
        #region constructor        

        public static IMeshPrimitiveDecoder<int> Create(SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> primitive, GLTFMaterialsFactory materials)
        {
            var material = materials.UseMaterial(primitive.Material);

            return new _MeshPrimitiveDecoder(primitive, material);
        }
        private _MeshPrimitiveDecoder(SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> primitive, int materialIndex)
        {
            _Source = primitive;
            _MaterialIndex = materialIndex;
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> _Source;
        private readonly int _MaterialIndex;

        #endregion

        #region properties

        public int Material => _MaterialIndex;

        public int VertexCount => _Source.VertexCount;

        public int MorphTargetsCount => 0;

        public int ColorsCount => _Source.ColorsCount;

        public int TexCoordsCount => _Source.TexCoordsCount;

        public int JointsWeightsCount => _Source.JointsWeightsCount == 0 ? 0 : 4;

        public IEnumerable<(int A, int B)> LineIndices => Enumerable.Empty<(int A, int B)>();

        public IEnumerable<(int A, int B, int C)> TriangleIndices => _Source.TriangleIndices;

        #endregion

        #region vertex API

        public XNAV3 GetPosition(int vertexIndex) { return _Source.GetPosition(vertexIndex).ToXNA(); }
        public XNAV3 GetNormal(int vertexIndex) { return _Source.GetNormal(vertexIndex).ToXNA(); }
        public XNAV4 GetTangent(int vertexIndex) { return _Source.GetTangent(vertexIndex).ToXNA(); }
        public VERTEXINFLUENCES GetSkinWeights(int vertexIndex)
        {
            var sparse = _Source
                .GetSkinWeights(vertexIndex)
                .GetTrimmed(4);

            var indices = new XNAV4(sparse.Index0, sparse.Index1, sparse.Index2, sparse.Index3);
            var weights = new XNAV4(sparse.Weight0, sparse.Weight1, sparse.Weight2, sparse.Weight3);

            return new VERTEXINFLUENCES(indices, weights);
        }

        

        public XNAV4 GetColor(int vertexIndex, int colorSetIndex) { return _Source.GetColor(vertexIndex, colorSetIndex).ToXNA(); }
        public XNAV2 GetTextureCoord(int vertexIndex, int textureSetIndex) { return _Source.GetTextureCoord(vertexIndex, textureSetIndex).ToXNA(); }


        public IReadOnlyList<XNAV3> GetNormalDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetPositionDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetTangentDeltas(int vertexIndex) { throw new NotImplementedException(); }

        #endregion
    }
}
