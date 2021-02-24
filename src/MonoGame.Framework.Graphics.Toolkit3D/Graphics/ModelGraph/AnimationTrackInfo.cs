using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics.Graphics.ModelGraph
{
    public class AnimationTrackInfo : BaseTemplate
    {
        public AnimationTrackInfo(string name, object tag, float duration)
            : base(name, tag)
        {            
            Duration = duration;
        }
        public float Duration { get; private set; }
    }
}
