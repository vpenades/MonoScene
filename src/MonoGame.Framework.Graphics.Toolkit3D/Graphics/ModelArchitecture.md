### ModelTemplate Overview

When comparing MonoGame with the new architecture, the object that would mirror [MonoGame´s Model class](https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Graphics/Model.cs),
is the [ModelTemplate class](ModelGraph/ModelTemplate.cs), it represents essentially the same thing;
a _stateless resource_ representing a 3D object.

But, unlike MonoGame's Model, [ModelTemplate class](ModelGraph/ModelTemplate.cs) is,
by default, contained within a [ModelCollection class](ModelGraph/ModelCollection.cs), so when
you load a glTF, you're not loading a single ModelTemplate, but a ModelCollection. In most
cases, loading a glTF will produce a ModelCollection with a single ModelTemplate inside, so,
why making it so complicated?

The answer is _resource management_: glTF has a very smart architecture that allows reusing
textures, materials, meshes and animations across multiple models, up to the point that
we can have multiple models stored in a single file, all of them sharing the same index and
vertex buffer.

That's why, the ModelCollection is more than a simple list of ModelTemplate objects, it also
holds the resources shared by the models.

```c#
    ModelCollection
      ┣━ Shared Meshes (n)
      ┣━ Shared Armatures (n)
      ┗━ Models Templates (n)
```

With this design, we can have many models reusing expensive resources like meshes and armatures.

BTW, Armatures are the skeletons for animated characters, they're expensive resources because they
also hold the animation curves of each node.

So an example of resource reusing, we can have something like this:

```c#
    ModelCollection
      ┣━ Shared Meshes
      ┃   ┣━ Mesh[0]
      ┃   ┣━ Mesh[1]
      ┃   ┣━ Mesh[2]
      ┃   ┗━ Mesh[3]
      ┣━ Shared Armatures
      ┃   ┗━ Armature[0]
      ┗━ Models Templates
          ┣━ Model[0], using Meshes[0,2] and Armature[0]
          ┣━ Model[1], using Meshes[1,2] and Armature[0]
          ┗━ Model[2], using Meshes[1,3] and Armature[0]
```

Since glTF (and probably other newer 3D formats) supports this resource management
architecture, I think it's very convenient to support it too at runtime level.

It is also worth to notice that currently, the classes are designed so all the
disposable resources are concentrated in the shared MeshCollection, so it's the
only part of the whole ModelCollection that needs to be disposed. Armatures and
ModelTemplates are made of regular managed classes, so they don't need to
implement IDisposable. In fact, it is possible to switch the Meshes of a
ModelCollection at runtime.


### ModelInstance overview

The first thing you'll notice when looking at the new classes is that
for some classes, there's two class variants

|Template class| Instance class|
|-|-|
|[ModelTemplate](ModelGraph/ModelTemplate.cs)|[ModelInstance](ModelGraph/ModelInstance.cs)|
|[DrawableTemplate](ModelGraph/DrawableTemplate.cs)|[DrawableInstance](ModelGraph/DrawableInstance.cs)|
|[ArmatureTemplate](ModelGraph/ArmatureTemplate.cs)|[ArmatureInstance](ModelGraph/ArmatureInstance.cs)|
|[NodeTemplate](ModelGraph/NodeTemplate.cs)|[NodeInstance](ModelGraph/NodeInstance.cs)|


So what's this about?

Well, if you compare with the original MonoGame's Model, it was essentially a content object, _a resource_
so in order to draw it on screen multiple times, you need to keep the world transforms of the instances
of that model somewhere, typically in custom structures. This is fine because in general, MG's Model has
been used most of the time as a rigid object, and it only needs a matrix to be rendered somewhere. It is
true that MG's Model supports skinning, but it was rarely used because it depended on the developer to
keep the skeleton transform state in custom structures that need to match the skeleton of the model. Because
MG's Model didn't hold the actual animations nor the structures to animate them, MG's Model was essentially
crippled to support animations out of the box.

To support animations _out of the box_ you need some state object that keeps the animation state of every
instance of the object on screen. You might have a model resource representing a _soldier_ and you want to
render it on screen multiple times, but each model drawing will be in a different state; one drawing will
represent the soldier walking, while another will represent it running.

In essence, the Template classes are _stateless resources_, and the Instance classes are the _state_ of a specific
representation of the source TemplateModel on screen.

Some might say that this is too much, that in order to render an animated character on screen you only need
the world transform, the animation track name, and the animation time, which can easily be passed to an
extended Draw call. This might be true for basic scenarios, but for more complex use cases it does not hold.

by having a specific set of objects that hold the state of a rigged character I can set a pose from a track
and time, but also blend multiple tracks, and after that, programatically modify the local transform of a
specific bone, or apply IK to some bone chains. This is the sort of stuff real games need to do, for which
you need a _state_ object.

In practice you load a TemplateModel as a resource, from which you create an instance every time you need
to draw it on screen (no need to create it per frame, you keep it as that instance is active)

The instance classes are designed to be as lightweight as possible, they don't have disposable objects, so
they're easy to create. But even is that is a problem to the GC, pooling strategies can be explored.

Now, a ModelInstance is

### Armatures and Animations

[Armatures](ModelGraph/ArmatureTemplate.cs) (or Skeletons), define a collection of Nodes with a hierarchical relationship.

Within an Armature you can find a table of nodes, the table has been flattened and sorted so parent nodes
must always appear before their children. This makes armature evaluation easier since there's no need
to traverse a complex hierarchy.

A Specific node can contain either one of these two:
- A Fixed Transform Matrix
- A set of properties representing:
  - S: A Vector3 Scale
  - R: A Quaternion Rotation
  - T: A Vector3 Translation

When using the SRT properties, each property can have a list of curves, one for every animation track.

Notice that Armatures are defined by two classes:

- [ArmatureTemplate](ModelGraph/ArmatureTemplate.cs)
  - Stateless
  - Represents the initial state of the model
  - Define the hierarchical relationship between parent and child nodes.
  - Contains the animation curves for all the animation tracks (which in same cases, can be a LOT of data)
  - 
- [ArmatureInstance](ModelGraph/ArmatureInstance.cs)
  - Statefull and lightwight.
  - Reference a ArmatureTemplate from where it gets the [ICurveEvaluator interface](../ICurveEvaluator.cs) evaluators.
  - Contains the evaluated local and world transform matrices for each node.  
  - Expose APIs to set the node matrices individually, or as a whole, defining an animation track and time.
  - The collection of world transforms represent the current, evaluated "pose" of the skeleton at a given time.
  - This object is the one from where the mesh skinning takes the world transform matrices.


### Drawing Commands

Each ModelTemplate has a list of [DrawableTemplate](ModelGraph/DrawableTemplate.cs) objects.

A [DrawableTemplate](ModelGraph/DrawableTemplate.cs) can be seen as drawing command, and it basically
tells to render a specific Mesh from a MeshCollection, in a specific location in the scene, defined
by one or more nodes:

```c
MeshCollection
                ⬊
                  DrawableTemplate ⮕ DrawableInstance: Draw Mesh[3] at Node[4].WorldTransform
                ⬈
ArmatureInstance
```







