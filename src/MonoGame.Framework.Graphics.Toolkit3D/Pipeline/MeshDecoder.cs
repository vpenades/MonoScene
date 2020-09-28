using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XY = Microsoft.Xna.Framework.Vector2;
using XYZ = Microsoft.Xna.Framework.Vector3;
using XYZW = Microsoft.Xna.Framework.Vector4;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public interface IMeshDecoder<TMaterial>
        where TMaterial : class
    {
        string Name { get; }        
        IReadOnlyList<IMeshPrimitiveDecoder<TMaterial>> Primitives { get; }

        Object Tag { get; }
    }    

    public interface IMeshPrimitiveDecoder<TMaterial> : IMeshPrimitiveDecoder
        where TMaterial : class
    {
        TMaterial Material { get; }
    }

    public interface IMeshPrimitiveDecoder
    {
        #region properties

        /// <summary>
        /// Gets a value indicating the total number of vertices for this primitive.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Gets a value indicating the total number of morph targets for this primitive.
        /// </summary>
        int MorphTargetsCount { get; }

        /// <summary>
        /// Gets a value indicating the number of color vertex attributes.
        /// In the range of 0 to 2.
        /// </summary>
        int ColorsCount { get; }

        /// <summary>
        /// Gets a value indicating the number of texture coordinate vertex attributes.
        /// In the range of 0 to 2.
        /// </summary>
        int TexCoordsCount { get; }

        /// <summary>
        /// Gets a value indicating the number of skinning joint-weight attributes.
        /// The values can be 0, 4 or 8.
        /// </summary>
        int JointsWeightsCount { get; }

        /// <summary>
        /// Gets a sequence of tuples where each item represents the vertex indices of a line.
        /// </summary>
        IEnumerable<(int A, int B)> LineIndices { get; }

        /// <summary>
        /// Gets a sequence of tuples where each item represents the vertex indices of a triangle.
        /// </summary>
        IEnumerable<(int A, int B, int C)> TriangleIndices { get; }

        #endregion

        #region API

        XYZ GetPosition(int vertexIndex);

        XYZ GetNormal(int vertexIndex);

        XYZW GetTangent(int vertexIndex);

        IReadOnlyList<XYZ> GetPositionDeltas(int vertexIndex);

        IReadOnlyList<XYZ> GetNormalDeltas(int vertexIndex);

        IReadOnlyList<XYZ> GetTangentDeltas(int vertexIndex);

        XY GetTextureCoord(int vertexIndex, int textureSetIndex);

        XYZW GetColor(int vertexIndex, int colorSetIndex);

        VertexSkinning GetSkinWeights(int vertexIndex);

        #endregion
    }

    public struct VertexSkinning
    {
        public Framework.Graphics.PackedVector.Short4 Indices;
        public XYZW Weights;
    }
}
