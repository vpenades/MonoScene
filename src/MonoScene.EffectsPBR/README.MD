### MonoGame.Framework.Graphics.PBR

Extends Monogame with additional PBR effects:

- AnimatedEffect (abstract base class)
  - UnlitEffect
  - PBREffect (abstract)
    - PBRMetallicRoughnessEffect
    - PBRSpecularGlossinessEffect

These effects are designed, but not limited, to provide the neccesary shader features
required by glTF format.

#### Supported Vertex shader features

##### Rigid vertices

glTF defines the classic Position-Normal-Tangent vertex attributes.

##### Skinned vertices

glTF supports standard skinning with 4 weights and 4 bone indices,
but it also supports extended skinning with 8 weights and 8 bone indices.

Monogame only supports standard skinning.

Also, glTF supports an _unlimited_ number of bones, which means that engines that
have limited constant buffers may fail to load models with a large number of bones.

Monogame's in-built SkinnedEffect has a limit of 72 bones, which by today standards
it's a bit low.

The recomendation is to use an independent constant buffer for bones.

##### Morphed vertices

Again, glTF supports an unlimited number of morph targets.

Unfortunately, MonoGame lacks the required graphics features needed to perform morphing,
so it will not be implemented in the current integration. [See #7341](https://github.com/MonoGame/MonoGame/issues/7341)






