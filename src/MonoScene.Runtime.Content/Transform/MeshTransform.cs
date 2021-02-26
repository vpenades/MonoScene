using System;
using System.Collections.Generic;
using System.Text;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using VERTEXINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;
using MORPHINFLUENCES = Microsoft.Xna.Framework.Graphics.PackedVector.BoneInfluences;

namespace MonoScene.Graphics
{   
    /// <summary>
    /// Represents the matrices (in model space) of a hierarchical armature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the source from where <see cref="IMeshTransform"/> takes<br/>
    /// the matrices for updating its internal state.
    /// </para>
    /// <para>
    /// Implemented by ArmatureInstance.
    /// </para>
    /// </remarks>
    public interface IArmatureTransform
    {
        // MORPHINFLUENCES GetMorphState(int nodeIndex);
        XNAMAT GetModelMatrix(int nodeIndex);
    }

    /// <summary>
    /// Interface for a mesh transform agent
    /// </summary>
    public interface IMeshTransform
    {
        /// <summary>
        /// Updates the current state of this transform.
        /// </summary>
        /// <param name="armature"></param>
        void Update(IArmatureTransform armature);

        /// <summary>
        /// Tries to get the current rigid matrix (in model space) of this transform.
        /// </summary>
        /// <param name="modelMatrix">A model matrix.</param>
        /// <returns>true if a matrix is available, false otherwise</returns>
        /// <remarks>
        /// Typically, this method returns true for rigid transforms, and false for skinned transforms.
        /// </remarks>
        bool TryGetModelMatrix(out XNAMAT modelMatrix);

        /// <summary>
        /// Tries to get the current skinned matrices (in model space) of this transform.
        /// </summary>
        /// <returns>An array of matrices, or null</returns>
        /// <remarks>
        /// Typically, this method returns null for rigid transform, and a non empty array for skinned transforms.
        /// </remarks>
        XNAMAT[] TryGetSkinMatrices();

        /// <summary>
        /// Gets a value indicating whether the current <see cref="IMeshTransform"/> will render visible geometry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this property is false, a runtime should skip rendering any geometry using<br/>
        /// this <see cref="IMeshTransform"/> instance, since it will not be visible anyway.
        /// </para>
        /// <para>
        /// This property is true most of the time, but it can become false when<br/>
        /// the ModelMatrix has a scale of 0 in any of its axes (determinant = 0).
        /// </para>
        /// <para>
        /// This feature can be used by an animation system to simulate<br/>
        /// visibility by setting the scale of the transform to 0.
        /// </para>
        /// </remarks>
        bool Visible { get; }

        /// <summary>
        /// Gets a value indicating whether the triangles need to be flipped to render correctly.
        /// </summary>
        /// <remarks>
        /// When this property is true, the runtime that renders the triangles should inverse the face culling.<br/>
        /// This usually happens when the Model Matrix is negative, either because it has a negative<br/>
        /// scale or because a mirror transform has been applied.
        /// </remarks>
        bool FlipFaces { get; }
        
        /// <summary>
        /// Transforms a vertex position from local mesh space to world space.
        /// </summary>
        /// <param name="position">The local position of the vertex.</param>
        /// <param name="positionDeltas">The local position deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A position in world space.</returns>
        XNAV3 TransformPosition(XNAV3 position, IReadOnlyList<XNAV3> positionDeltas, in VERTEXINFLUENCES skinWeights);

        /// <summary>
        /// Transforms a vertex normal from local mesh space to world space.
        /// </summary>
        /// <param name="normal">The local normal of the vertex.</param>
        /// <param name="normalDeltas">The local normal deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A normal in world space.</returns>
        XNAV3 TransformNormal(XNAV3 normal, IReadOnlyList<XNAV3> normalDeltas, in VERTEXINFLUENCES skinWeights);

        /// <summary>
        /// Transforms a vertex tangent from local mesh space to world space.
        /// </summary>
        /// <param name="tangent">The tangent normal of the vertex.</param>
        /// <param name="tangentDeltas">The local tangent deltas of the vertex, one for each morph target, or null.</param>
        /// <param name="skinWeights">The skin weights of the vertex, or default.</param>
        /// <returns>A tangent in world space.</returns>
        XNAV4 TransformTangent(XNAV4 tangent, IReadOnlyList<XNAV3> tangentDeltas, in VERTEXINFLUENCES skinWeights);

        XNAV4 MorphColors(XNAV4 color, IReadOnlyList<XNAV4> morphTargets);        
    }     
}
