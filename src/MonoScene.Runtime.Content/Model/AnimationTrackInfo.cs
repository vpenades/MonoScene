using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics.Content
{
    [System.Diagnostics.DebuggerDisplay("{Name} {Duration}s")]
    public class AnimationTrackInfo : BaseContent
    {
        public AnimationTrackInfo(string name, object tag, float duration)
            : base(name, tag)
        {            
            Duration = duration;
        }
        public float Duration { get; private set; }
    }
}
