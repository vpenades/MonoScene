#### MonoScene.Runtime.Content

This project contains all the classes required to define a 3d model in memory,
and eventually, to serialize it (Most probably using ProtoBuffer)

The architecture loosely replicates the architecture of the glTF standard,
which is a feature rich model architecture.

Architecture overview:

- [ModelCollectionContent](ModelCollectionContent.cs)
  - Resources
    - [MeshCollectionContent](Meshes/MeshCollectionContent.cs)
      - [MeshContent](Meshes/MeshContent.cs) [n]
        - [MeshPartContent](Meshes/MeshContent.cs) [n]
          - [MeshGeometryContent](Meshes/MeshContent.cs)
      - [VertexBufferContent](Meshes/VertexBufferContent.cs) [n]
      - [IndexBufferContent](Meshes/IndexBufferContent.cs) [n]
      - [MaterialContent](Materials/MaterialContent.cs) [n]
        - [MaterialChannelContent](Materials/MaterialChannelContent.cs) [n]
          - [SamplerStateContent](Materials/SamplerStateContent.cs)
          - Texture
    - [ArmatureContent](Armature/ArmatureContent.cs) [n]
      - [NodeContent](Armature/NodeContent.cs) [n] 
        - [Scale Animations](AnimatableProperty.cs)    
        - [Rotation Animations](AnimatableProperty.cs)    
        - [Translation Animations](AnimatableProperty.cs)    
  - Graph
    - [ModelContent](Model/ModelContent.cs) [n] __<- entry point for a single model__
      - [DrawableContent](Model/DrawableContent.cs) [n]
        - RigidDrawableContent - References a rigid mesh in resources
        - SkinnedDrawableContent - References a skinned mesh in resources


#### Comparing with MonoGame architecture

MonoGame's content classes are designed to be used by the content processor pipeline,
which can only be used at build time. This prevents loading public models at runtime.

MonoScene's content classes are designed to be used both for content processing at
build time, and also for content loading and procedural creation at runtime.

One important aspect of 3D models architecture is _resource sharing_; in MonoGame, this
is resolved at build time by the content processor pipeline, but since we require to load
models at runtime too, we cannot process the models as deeply as an offline pipeline, that
means that resource sharing is isolated to the context of a single source model.