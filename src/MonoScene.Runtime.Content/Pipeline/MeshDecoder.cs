using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;

namespace MonoScene.Graphics.Pipeline
{
    /// <summary>
    /// Interface used by importers to wrap the imported mesh.
    /// </summary>
    /// <typeparam name="TMaterial"></typeparam>
    public interface IMeshDecoder<TMaterial>
        where TMaterial : class
    {
        /// <summary>
        /// The mesh name.
        /// </summary>
        string Name { get; }

        //
        Object Tag { get; }
        IReadOnlyList<IMeshPrimitiveDecoder<TMaterial>> Primitives { get; }        
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

        XNAV3 GetPosition(int vertexIndex);

        XNAV3 GetNormal(int vertexIndex);

        XNAV4 GetTangent(int vertexIndex);

        IReadOnlyList<XNAV3> GetPositionDeltas(int vertexIndex);

        IReadOnlyList<XNAV3> GetNormalDeltas(int vertexIndex);

        IReadOnlyList<XNAV3> GetTangentDeltas(int vertexIndex);

        XNAV2 GetTextureCoord(int vertexIndex, int textureSetIndex);

        XNAV4 GetColor(int vertexIndex, int colorSetIndex);

        VertexInfluences GetSkinWeights(int vertexIndex);

        #endregion
    }    
}
