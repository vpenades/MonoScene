using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoScene.Graphics.Content;

using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAMAT = Microsoft.Xna.Framework.Matrix;
using XNAQUAT = Microsoft.Xna.Framework.Quaternion;

namespace MonoScene.Graphics.Pipeline
{
    public abstract class ArmatureFactory<TNode>
    {
        #region data

        private readonly List<AnimationTrackInfo> _AnimationTracks = new List<AnimationTrackInfo>();
        private readonly List<NodeContent> _Nodes = new List<NodeContent>();
        private readonly Dictionary<TNode, NodeContent> _Map = new Dictionary<TNode, NodeContent>();

        #endregion

        #region API

        public void SetAnimationTrack(int index, string name, object tag, float duration)
        {
            while (_AnimationTracks.Count <= index) _AnimationTracks.Add(new AnimationTrackInfo(null, null, 0));

            _AnimationTracks[index] = new AnimationTrackInfo(name, tag, duration);
        }

        public void AddRoot(TNode node)
        {
            AddNodeRescursive(node, -1);
        }

        protected abstract string GetName(TNode node);
        protected abstract Object GetTag(TNode node);
        protected abstract IEnumerable<TNode> GetChildren(TNode node);
        protected abstract XNAMAT GetLocalMatrix(TNode node);
        protected abstract AnimatableProperty<XNAV3> GetScale(TNode node);
        protected abstract AnimatableProperty<XNAQUAT> GetRotation(TNode node);
        protected abstract AnimatableProperty<XNAV3> GetTranslation(TNode node);


        public NodeContent GetNode(TNode srcNode) => _Map[srcNode];        

        public DrawableContent CreateRigidDrawableContent(int meshIndex, TNode node)
        {
            var n = GetNode(node);
            return DrawableContent.CreateRigid(meshIndex, n);
        }

        public DrawableContent CreateSkinnedDrawableContent(int meshIndex, TNode container, (TNode, XNAMAT)[] bones)
        {
            var xbones = bones
                .Select(item => (GetNode(item.Item1), item.Item2))
                .ToArray();

            return DrawableContent.CreateSkinned(meshIndex, null, xbones);
        }

        public ArmatureContent CreateArmature() { return new ArmatureContent(_Nodes.ToArray(), _AnimationTracks.ToArray()); }

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
            
            var dst = new NodeContent(thisIdx, parentIndex, childIndices.ToArray());
            dst.Name = GetName(src);
            dst.Tag = GetTag(src);

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
