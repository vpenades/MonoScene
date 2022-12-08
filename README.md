# MonoScene Framework
#### A MonoGame 3D Model alternative.

![MonoGame Demo](MonoGameDemoPBR.jpg)

### ⚠️ Before using MonoScene ⚠️

This library was my attempt to modernize MonoGame's 3D support, and eventually to have
something useful to use in Virtual Reality projects, provided MonoGame would have supported
VR at some point.

Eventually, I hit the limits of what MonoGame can do with graphics. MonoGame prioritizes
backwards compatibility and consoles over adding new features, which is not neccesarily
a bad thing, but it's indeed hurting MonoGame to keep up with the competition.

To overcome MonoGame limitations, the path many choose is to fork MonoGame to add additional
features, __but IMHO this only leads to a fragmented and broken ecosystem__. So it's not
a good solution either.

My objective is to keep this project compatible with the official MonoGame, so I'm limited
to what its outdated graphics subsystem can do, which led me to __stop development of this
library (except for maintenance) until MonoGame improves on the missing features or
another fork takes the lead.__

### Overview

MonoScene is a framework that replaces the very outdated 3D model architecture
currently available in MonoGame, providing a number of much needed features:

- PBR effect shaders.
- Full skeleton animation.
- Loading asset models at runtime (No Pipeline required)
- Basic Scene management

### Requirements

- Visual Studio 2022 required for Net6.
- MonoGame __3.8.1.303__ for Net6.0
- MonoGame __3.8.1.1825-develop__ for NetStandard 2.1

This project requires compiling a huge number of techniques that surpassed what was possible
with MonoGame 3.8.0, so it absolutely needs the shader compiler that comes with MonoGame 3.8.1

This framework also has third party dependencies that might prevent this project
to be ported to consoles.

### Limitations

##### MonoGame's Content Pipeline

Right now, It is **not possible** to import glTFs through MonoGame's content pipeline.

Unfortunately, MonoGame's (XNA) content pipeline is designed in a way that prevents processing many of
the glTF models in the wild, also MonoGame's built in model importer uses Assimp, which is only able
to handle a small subset of glTF models, and it's missing lots of key features.

So glTFs need to be loaded at runtime, which means that only projects able to consume `MonoScene.Pipeline.GLTF`
library and its dependencies will be able to load glTFs.

##### Textures

Textures are loaded using Monogame's `Texture2D.FromStream`, which means all of its limitations apply:
- No Mipmaps
- glTF texture formats WEBP and Universal Basis KTX2 are not supported.

Not having a content pipeline means the same asset is loaded on all platforms, which might not be optimal.
This could be mitigated if KTX2 texture format would be supported, because it's a texture compression
format similar to DDS with cross platform capabilities.

##### Animations

- Due to limitations in the Effects and Shaders API of MonoGame, glTF's morphing features are not supported.
- Maximum number of bones is limited to __72 bones__ with SkinnedEffect (as usual) and __128 bones__ with PBR effects.

##### Scene

Scene management is extremely basic and it's more an example than an actual framework, and it's missing some key features:
- Scene culling for models outside the view area
- LOD management
- Animation blending (it's supported under the hood, but not exposed yet)

##### Shaders

Shaders are missing three features that I think are key to be able to do decent looking renders:
- Shadows
- Glow effect
- glTF style Vertex Morphing

Vertex Morphing is not supported because it exceeds what MonoGame can do right now.
Shadows and Glow might be possible, though.

I'm not an shaders expert, so it's unlikely I'll do more work with the shaders, I'll try to add some
documentation, though.

#### Credits

- [MonoGame](https://github.com/MonoGame/MonoGame)
- [PBR Shaders from Khronos Viewer](https://github.com/KhronosGroup/glTF-Sample-Viewer)
- [SharpGLTF library](https://github.com/vpenades/SharpGLTF)
- [KNI engine for VR Support](https://github.com/kniengine/kni/)