using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.Graphics.ModelGraph;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class ArmatureFactory<TNode>
    {
        #region data

        private readonly List<AnimationTrackInfo> _AnimationTracks = new List<AnimationTrackInfo>();

        private readonly List<NodeTemplate> _Nodes = new List<NodeTemplate>();
        private readonly Dictionary<TNode, NodeTemplate> _Map = new Dictionary<TNode, NodeTemplate>();

        #endregion

        #region API

        public void SetAnimationTrack(int index, string name, float duration)
        {
            while (_AnimationTracks.Count <= index) _AnimationTracks.Add(new AnimationTrackInfo(null,0));

            _AnimationTracks[index] = new AnimationTrackInfo(name, duration);
        }

        public void AddRoot(TNode node)
        {
            AddNodeRescursive(node, -1);
        }

        protected abstract string GetName(TNode node);
        protected abstract IEnumerable<TNode> GetChildren(TNode node);
        protected abstract Matrix GetLocalMatrix(TNode node);
        protected abstract AnimatableProperty<Vector3> GetScale(TNode node);
        protected abstract AnimatableProperty<Quaternion> GetRotation(TNode node);
        protected abstract AnimatableProperty<Vector3> GetTranslation(TNode node);


        public NodeTemplate GetNode(TNode srcNode) => _Map[srcNode];

        public IDrawableTemplate CreateRigidDrawable(int meshIndex, TNode node)
        {
            var n = GetNode(node);
            var d = new RigidDrawableTemplate(meshIndex, n);
            return d;
        }

        public IDrawableTemplate CreateSkinnedDrawable(int meshIndex, TNode container, (TNode,Matrix)[] bones)
        {
            var xbones = bones
                .Select(item => (GetNode(item.Item1), item.Item2))
                .ToArray();

            var d = new SkinnedDrawableTemplate(meshIndex, null, GetNode(container).Name, xbones);

            return d;
        }

        public ArmatureTemplate CreateArmature() { return new ArmatureTemplate(_Nodes.ToArray(), _AnimationTracks.ToArray()); }

        #endregion

        #region core

        private int AddNodeRescursive(TNode src, int parentIndex)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (_Map.ContainsKey(src)) throw new ArgumentException("already exists");
            if (parentIndex >= _Nodes.Count) throw new ArgumentOutOfRangeException(nameof(parentIndex));

            var thisIdx = _Nodes.Count;
            _Nodes.Add(null);

            var childIndices = new List<int>();
            
            
            foreach(var child in GetChildren(src))
            {
                var childIndex = AddNodeRescursive(child, thisIdx);
                childIndices.Add(childIndex);
            }

            
            var dst = new NodeTemplate(thisIdx, parentIndex, childIndices.ToArray());
            dst.Name = GetName(src);

            dst.SetLocalMatrix(GetLocalMatrix(src));

            var s = GetScale(src);
            var r = GetRotation(src);
            var t = GetTranslation(src);
            dst.SetLocalTransform(s, r, t);

            _Nodes[thisIdx] = dst;
            _Map[src] = dst;

            // TODO: ensure there's enough animation tracks.

            return thisIdx;
        }        

        #endregion        
    }
}
