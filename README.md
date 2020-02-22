# MonoGame realtime rendering demo

![MonoGame Demo](MonoGameDemo.jpg)

### Overview

This is an example of loading and rendering glTF files with MonoGame.

This demo is just a stripped down copy of the original example code [which can be found here](https://github.com/vpenades/SharpGLTF/tree/master/examples/MonoGameScene). I've created this repository to showcase the minimum requirements to make this example work.

Although we're using this library in production code in our internal projects, this is a library made out of neccesity, due to the lack of proper glTF support in MonoGame.

The primary objectives of this demo is to showcase that MonoGame is indeed able to handle glTF files, and to provide a temporary solution for those that need some glTF support right now. But our hope is that over time, MonoGame will be able to handle glTF files on its own, so this library will no longer be needed.

### Running the demo:

The original project

The project depends on [SharpGLTF.Runtime.MonoGame](src/SharpGLTF.Runtime.MonoGame), which also depends on [SharpGLTF.Core](https://www.nuget.org/packages/SharpGLTF.Core), that's everything you need to load and render glTF models into MonoGame.

### Demo limitations

MonoGame typically preprocesses all graphics resources through its content pipeline, and all assets are converted to XNB files, which is what the runtime is able to load.

This is not the case of this demo; the glTF files are loaded at runtime without any preprocessing.

Also for simplicity, the demo uses the default in-built BasicEffect and SkinnedEffect shaders, which date from the years of DirectX9, this limits the capabilities of glTF in these areas:
- PBR materials are not supported, and are converted/downgraded to basic materials.
- Morphing is not supported.
- Skinned meshes with more than 72 bones will probably fail.

Since the library depends upon SharpGLTF.Core, which does heavy use of advanced c# features, it is currently aimed for Monogame on Desktop and probably Android, But any project requiring BRUTE, will not be able to use this project.



