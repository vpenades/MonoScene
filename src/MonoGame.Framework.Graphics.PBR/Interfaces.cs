using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectBones // it could be great if SkinnedEffect implemented this.
    {
        void SetBoneTransforms(Matrix[] boneTransforms);
    }
}
