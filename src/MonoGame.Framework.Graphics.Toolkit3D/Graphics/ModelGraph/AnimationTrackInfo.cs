using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics.Graphics.ModelGraph
{
    public class AnimationTrackInfo
    {
        public AnimationTrackInfo(string name, float duration)
        {
            Name = name;
            Duration = duration;
        }

        public string Name { get; private set; }
        public float Duration { get; private set; }
    }
}
