using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScene.Graphics
{
    public class AnimationTrackInfo : Content.BaseContent
    {
        public AnimationTrackInfo(string name, object tag, float duration)
            : base(name, tag)
        {            
            Duration = duration;
        }
        public float Duration { get; private set; }
    }
}
