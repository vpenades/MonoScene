@echo off
setlocal

rem install https://www.nuget.org/packages/dotnet-mgfxc/

del *.mgfxo

mgfxc ..\Unlit.Permutations.fx Unlit.ogl.mgfxo
mgfxc ..\Unlit.Permutations.fx Unlit.dx11.mgfxo /Profile:DirectX_11

mgfxc ..\PBR.Permutations.fx MetallicRoughnessEffect.ogl.mgfxo /Defines:MATERIAL_METALLICROUGHNESS
mgfxc ..\PBR.Permutations.fx MetallicRoughnessEffect.dx11.mgfxo /Defines:MATERIAL_METALLICROUGHNESS /Profile:DirectX_11

mgfxc ..\PBR.Permutations.fx SpecularGlossinessEffect.ogl.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS
mgfxc ..\PBR.Permutations.fx SpecularGlossinessEffect.dx11.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS /Profile:DirectX_11

endlocal
pause