using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using MonoScene.Graphics.Content;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;


namespace MonoScene.Graphics.Pipeline
{
    readonly struct _MeshDecoder : IMeshDecoder<MaterialContent>
    {
        #region constructor
        public _MeshDecoder(SharpGLTF.Runtime.IMeshDecoder<SharpGLTF.Schema2.Material> mesh, IReadOnlyList<MaterialContent> materials, object tag)
        {
            _Source = mesh;
            _Primitives = mesh.Primitives.Select(item => _MeshPrimitiveDecoder.Create(item, materials)).ToArray();
            _Tag = tag;
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshDecoder<SharpGLTF.Schema2.Material> _Source;
        private readonly IMeshPrimitiveDecoder<MaterialContent>[] _Primitives;
        private readonly object _Tag;

        #endregion

        #region properties

        public string Name => _Source.Name;
        public object Tag => _Tag;

        public IReadOnlyList<IMeshPrimitiveDecoder<MaterialContent>> Primitives => _Primitives;        

        #endregion
    }

    readonly struct _MeshPrimitiveDecoder : IMeshPrimitiveDecoder<MaterialContent>
    {
        #region constructor

        private static readonly MaterialContent _DefaultMaterial = new MaterialContent();

        public static IMeshPrimitiveDecoder<MaterialContent> Create(SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> primitive, IReadOnlyList<MaterialContent> materials)
        {
            var material = primitive.Material == null ? _DefaultMaterial : materials[primitive.Material.LogicalIndex];

            return new _MeshPrimitiveDecoder(primitive, material);
        }
        private _MeshPrimitiveDecoder(SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> primitive, MaterialContent material)
        {
            _Source = primitive;
            _Material = material;
        }

        #endregion

        #region data

        private readonly SharpGLTF.Runtime.IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> _Source;
        private readonly MaterialContent _Material;

        #endregion

        #region properties

        public MaterialContent Material => _Material;

        public int VertexCount => _Source.VertexCount;

        public int MorphTargetsCount => 0;

        public int ColorsCount => _Source.ColorsCount;

        public int TexCoordsCount => _Source.TexCoordsCount;

        public int JointsWeightsCount => _Source.JointsWeightsCount == 0 ? 0 : 4;

        public IEnumerable<(int A, int B)> LineIndices => Enumerable.Empty<(int A, int B)>();

        public IEnumerable<(int A, int B, int C)> TriangleIndices => _Source.TriangleIndices;

        #endregion

        #region vertex API

        public XNAV3 GetPosition(int vertexIndex) { return _Source.GetPosition(vertexIndex); }
        public XNAV3 GetNormal(int vertexIndex) { return _Source.GetNormal(vertexIndex); }
        public XNAV4 GetTangent(int vertexIndex) { return _Source.GetTangent(vertexIndex); }
        public VERTEXINFLUENCES GetSkinWeights(int vertexIndex)
        {
            var sparse = _Source
                .GetSkinWeights(vertexIndex)
                .GetReducedWeights(4);

            var indices = new XNAV4(sparse.Index0, sparse.Index1, sparse.Index2, sparse.Index3);
            var weights = new XNAV4(sparse.Weight0, sparse.Weight1, sparse.Weight2, sparse.Weight3);

            return new VERTEXINFLUENCES(indices, weights);
        }

        

        public XNAV4 GetColor(int vertexIndex, int colorSetIndex) { return _Source.GetColor(vertexIndex, colorSetIndex); }
        public XNAV2 GetTextureCoord(int vertexIndex, int textureSetIndex) { return _Source.GetTextureCoord(vertexIndex, textureSetIndex); }


        public IReadOnlyList<XNAV3> GetNormalDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetPositionDeltas(int vertexIndex) { throw new NotImplementedException(); }
        public IReadOnlyList<XNAV3> GetTangentDeltas(int vertexIndex) { throw new NotImplementedException(); }

        #endregion
    }
}
