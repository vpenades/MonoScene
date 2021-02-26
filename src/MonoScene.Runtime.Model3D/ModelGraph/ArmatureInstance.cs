using System;
using System.Collections.Generic;
using System.Linq;

using MonoScene.Graphics.Content;

using XNAMAT = Microsoft.Xna.Framework.Matrix;

namespace MonoScene.Graphics
{
    /// <summary>
    /// Represents an instanced state of <see cref="ArmatureContent"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="ArmatureContent"/> represents the layout, graph and<br/>
    /// initial state of a skeleton, and it's a READ ONLY OBJECT.
    /// </para>
    /// <para>
    /// Moving forward, an <see cref="ArmatureInstance"/> represents the current<br/>
    /// state of a an independent skeleton within a 3D scene, so each joint can be<br/>
    /// rotated independently from other instances, without affecting each other.
    /// </para>
    /// </remarks>
    public class ArmatureInstance : IArmatureTransform
    {
        #region lifecycle

        internal ArmatureInstance(ArmatureContent armature)
        {
            _ArmatureContent = armature;
            _NodeInstances = new NodeInstance[armature.Count];

            // no need to check arguments since they're supposedly pre-checked by ArmatureTemplate's constructor.

            for (var i = 0; i < _NodeInstances.Length; ++i)
            {
                var n = armature[i];
                var pidx = n.ParentIndex;
                var p = pidx < 0 ? null : _NodeInstances[pidx];
                _NodeInstances[i] = new NodeInstance(n, p);
            }            
        }

        #endregion

        #region data

        private ArmatureContent _ArmatureContent;
        private NodeInstance[] _NodeInstances;              

        #endregion

        #region properties

        /// <summary>
        /// Gets a list of all the <see cref="NodeInstance"/> nodes used by this <see cref="ModelInstance"/>.
        /// </summary>
        public IReadOnlyList<NodeInstance> LogicalNodes => _NodeInstances;

        /// <summary>
        /// Gets all the <see cref="NodeInstance"/> roots used by this <see cref="ModelInstance"/>.
        /// </summary>
        public IEnumerable<NodeInstance> VisualNodes => _NodeInstances.Where(item => item.VisualParent == null);

        /// <summary>
        /// Gets the total number of animation tracks for this instance.
        /// </summary>
        public int AnimationTracksCount => _ArmatureContent.Animations.Count;        

        #endregion

        #region API

        public int IndexOfNode(string nodeName)
        {
            for (int i = 0; i < _NodeInstances.Length; ++i)
            {
                if (_NodeInstances[i].Name == nodeName) return i;
            }

            return -1;
        }

        public void SetLocalMatrix(string name, XNAMAT localMatrix)
        {
            var n = LogicalNodes.FirstOrDefault(item => item.Name == name);
            if (n == null) return;
            n.LocalMatrix = localMatrix;
        }

        XNAMAT IArmatureTransform.GetModelMatrix(int index)
        {
            return LogicalNodes[index].ModelMatrix;
        }

        public void SetModelMatrix(string name, XNAMAT modelMatrix)
        {
            var n = LogicalNodes.FirstOrDefault(item => item.Name == name);
            if (n == null) return;
            n.ModelMatrix = modelMatrix;
        }

        public void SetPoseTransforms()
        {
            foreach (var n in _NodeInstances) n.SetPoseTransform();
        }

        public string NameOfTrack(int trackIndex) { return _ArmatureContent.Animations[trackIndex].Name; }

        public int IndexOfTrack(string name) { return _ArmatureContent.IndexOfTrack(name); }

        public float GetAnimationDuration(int trackIndex) { return _ArmatureContent.GetTrackDuration(trackIndex); }

        public void SetAnimationFrame(int trackIndex, float time, bool looped = true)
        {
            if (looped)
            {
                var duration = GetAnimationDuration(trackIndex);
                if (duration > 0) time %= duration;
            }

            foreach (var n in _NodeInstances) n.SetAnimationFrame(trackIndex, time);
        }

        public void SetAnimationFrame(params (int trackIndex, float Time, float Weight)[] blended)
        {
            SetAnimationFrame(_NodeInstances, blended);
        }

        public static void SetAnimationFrame(IEnumerable<NodeInstance> nodes, params (int TrackIdx, float Time, float Weight)[] blended)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            Span<int> tracks = stackalloc int[blended.Length];
            Span<float> times = stackalloc float[blended.Length];
            Span<float> weights = stackalloc float[blended.Length];

            float w = blended.Sum(item => item.Weight);

            w = w == 0 ? 1 : 1 / w;

            for (int i = 0; i < blended.Length; ++i)
            {
                tracks[i] = blended[i].TrackIdx;
                times[i] = blended[i].Time;
                weights[i] = blended[i].Weight * w;
            }

            foreach (var n in nodes) n.SetAnimationFrame(tracks, times, weights);
        }        

        #endregion
    }
}
