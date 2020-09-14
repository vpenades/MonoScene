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

For rigid geometry, glTF defines the classic Position-Normal-Tangent vertex attributes.

glTF also supports skinning and morphing, and it's important to notice that both are
supported at the same time. Yes, you can have a mesh that has morph targets, and then
it has a skin over the morph targets.

Unfortunately, MonoGame lacks the required graphics features needed to perform morphing,
so it will not be implemented in the current integration.






