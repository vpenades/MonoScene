
#### Effect Techniques

MonoGame uses the concept of techniques for creating shader effects.

Techniques represent permutations over a number of options that can typically set or unset.
As the effect grows more complex and more options are added, the number of possible
permutations grow exponentially, to a point it's almost impossible to write the techniques
manually.

#### T4 Templating System

There's a number of ways of writing the techniques permutations programatically, I choose to
use the T4 templating system of Visual Studio because it's integrated into the IDE, and
it runs automatically on saving the template.

For those not familiar with T4 templates, here's a small example on how they work:

```c#
<# for (int i = 0; i < 3; ++i) { #>
    Hello World <#= i #>
<# } #>
```

Will produce this output

    Hello World 0
    Hello World 1
    Hello World 2

So basically, macro code is enclosed within <# ... #> blocks and <#= .... #> evaluates the content as a string.

In order to add a new template into a `csproj` file, a common xml pattern is used,
where the generator and the generated file are cross referenced:

```xml
<ItemGroup>
    <None Update="code.tt">
      <LastGenOutput>code.gen.fx</LastGenOutput>
      <Generator>TextTemplatingFileGenerator</Generator>
    </None>
    <None Update="code.gen.fx">
      <DependentUpon>code.tt</DependentUpon>
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
    </None>    
</ItemGroup>

<ItemGroup>
    <!-- Enables T4 templating in a csproj file -->
    <Service Include="{508349b6-6b84-4df5-91f0-309beebad82d}" />
</ItemGroup>  
```

With this pattern it is recomended to include the generated file in version control,
because the output file is generated at edit time, not at compile time, so CI environments
might fail on missing files.


Editing TT template files can be eye tearing,
I suggest to install [Tim Maes T4 Editor](https://github.com/Tim-Maes/T4Editor)
And then adjust the colors in `Visual Studio Menu > Extensions > T4Editor > Adjust Colors`
to distingish between content code and template code.

#### What's left / missing

I am not an expert shader expert and maybe some optimizations are possible. My objective
has been to support as many glTF features as possible over performance.

The three biggest missing features are:
- Morph vertices, which I think are not possible with vanilla monogame.
- Shadows
- Glow effects


Morph vertices are not just a simple tweening, under glTF it's even possible
to have morphed and skinned vertices at the same time!

Shadows would need additional shader code and more technique permutations.

Glow would need another effect, similar to Unlit, to render the emissive textures
over a render target, to be used at a later stage.


#### Credits

The shaders have been lifted and adapted to monogame from [glTF-Sample-Viewer](https://github.com/KhronosGroup/glTF-Sample-Viewer/tree/master/source/shaders)