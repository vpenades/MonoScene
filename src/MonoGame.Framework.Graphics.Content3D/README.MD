#### Microsoft.Xna.Framework.Content.Runtime.Graphics

In this project you can find the helper classes that can be used to build the meshes
and materials of a 3D model.

Why `Content.Runtime.Graphics` namespace, instead of the classic `Content.Pipeline.Graphics` ?

The classes you usually find in the **Pipeline** namespace are designed so they can be used only within the
context of the MonoGame Content Builder tool (MGCB), which runs at compile time only. But The classes found in
the **Runtime** should be used not only at compile time, but also at runtime. This will allow, with some
limitations, to load third party formats at runtime.