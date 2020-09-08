using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectBones // it could be great if SkinnedEffect implemented this.
    {
        void SetBoneTransforms(Matrix[] boneTransforms);
    }
}
