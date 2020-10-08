#### Skinning considerations

First of all, it's important to undestand that glTF skinning architecture is quite versatile, and allows
defining a skinned rig in ways that might not be compatible with pre-existing engines skeletal systems.

In glTF, a skinned mesh is usually defined like this:


```
(fig 1)
Scene  
 ┣━🐒 SkinnedMesh     
 ┗━🦴 Hip
    ┗━🦴 Chest
       ┗━🦴 Neck
          ┣━🦴 LeftShoulder
          ┣━🦴 RightShoulder
          ┗━🦴 Head
```

But it can also be defined like this:

```
(fig 2)
Scene  
 ┗━🐒 SkinnedMesh     
    ┗━🦴 Hip
       ┗━🦴 Chest
          ┗━🦴 Neck
             ┣━🦴 LeftShoulder
             ┣━🦴 RightShoulder
             ┗━🦴 Head
```

or like this:

```
(fig 3)
Scene
 ┗━ RootNode
    ┣━🐒 SkinnedMesh     
    ┗━🦴 Hip
       ┗━🦴 Chest
          ┗━🦴 Neck
             ┣━🦴 LeftShoulder
             ┣━🦴 RightShoulder
             ┗━🦴 Head
```

And then, the `Mesh` object contains a  `Skin` definition that maps which nodes
of the scene graph influence the skin of this particular mesh.

Many engines expect the mesh to be the parent node of the skeleton, and the skeleton bones to
be child nodes of that root node containing the mesh, and also expect all the nodes to be used
as part of the skinning _in the same order as they're defined_, so there's no need for any kind
of skin mapping. Although this skeleton architecture is certainly possible with glTF, it's a
naive approach to skinning and also very limiting.

Here's a few use cases that are possible with glTF that would not be possible with a basic
skinning architecture without a skin mapping.

Case 1, Remember the RayMan character? 🖐👀

It was made of _floating parts_, and it did not have any elbows or knees in the _geometry_ , but
it certainly needed elbows and knees to support the whole skeleton animation hierarchy.

```
RayMan scene
 ┣━👀 RaymanMesh
 ┗━⚡ Hip
    ┗━👚 Chest
       ┗━⚙ Neck
          ┣━⚙ LeftShoulder
          ┃  ┗━⚙ LeftElbow
          ┃     ┗━🤚 LeftHand
          ┣━⚙ RightShoulder
          ┃  ┗━⚙ RightElbow
          ┃     ┗━🤚 RightHand
          ┗━👀 Head

Nodes with symbol ⚙ represent bones in the skeleton graph NOT used by the mesh skin.
```

Case 2: Variations

What if we want to have multiple sets of the same character, sharing the same skeleton and animations
to save memory?  with (fig 1) is not possible, because the skeleton and animations are
children of the root node which holds the mesh, so we either switch the mesh at runtime, which is ugly,
or we duplicate the whole tree.

glTF solves this problem by storing all the required resources like this:

```
glTF resources
 ┣━🐒 MonkeyMesh     
 ┣━🦧 OrangutanMesh     
 ┗━🦴 Hip
    ┗━🦴 Chest
       ┗━🦴 Neck
          ┣━🦴 LeftShoulder
          ┣━🦴 RightShoulder
          ┗━🦴 Head

```

And at render time, it uses _Scenes_ that map into the resources, saving a lot of memory:

```
Scene 0
 ┣━🐒 MonkeyMesh     
 ┗━🦴 Hip
    ┗━🦴 Chest
       ┗━🦴 Neck
          ┣━🦴 LeftShoulder
          ┣━🦴 RightShoulder
          ┗━🦴 Head
Scene 1
 ┣━🦧 OrangutanMesh     
 ┗━🦴 Hip
    ┗━🦴 Chest
       ┗━🦴 Neck
          ┣━🦴 LeftShoulder
          ┣━🦴 RightShoulder
          ┗━🦴 Head
```

This architecture can be useful in a number of scenarios:
- Different LOD levels of the same character.
- Different states of the same character: a fresh character and a half beaten character.


Case 3: Multiple Skins

Consider a skinned character that uses two meshes:
- naked body mesh
- clothes

Both meshes share the same skeleton, but although there's some bone overlapping,
the actual skinning mapping might not be exactly the same:

```
Scene 0
 ┣━🐒⓵ MonkeyMesh 
 ┣━🥼⓶ ClothesMesh
 ┗━⓵⓶ Hip
    ┗━⓵⓶ Chest
       ┗━⓵ Neck
          ┣━⓵⓶ LeftShoulder
          ┣━⓵⓶ RightShoulder
          ┗━⓵ Head
```

Furthermore, since MonkeyMesh⓵ and ClothesMesh⓶ might have been bound to a skin at different times during authoring,
their respective _Inverse Bind Matrices_ might be completely different.

Why an artist might want to do something that ugly?

One reason might simply be because he doesn't care and, after all, all authoring application do support this workflow.

The other is to have tight control over how joints bend at acute angles. This is a well known problem with human
articulations, it's specially noticeable on knees and elbows when they're bent over 90 degrees, looking like the
vertices are collapsing over themselves.