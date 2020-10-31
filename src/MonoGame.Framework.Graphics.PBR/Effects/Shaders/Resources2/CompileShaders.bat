@echo off
setlocal

rem install https://www.nuget.org/packages/dotnet-mgfxc/

del *.mgfxo

mgfxc ..\Techniques.Unlit.fx Unlit.ogl.mgfxo
mgfxc ..\Techniques.Unlit.fx Unlit.dx11.mgfxo /Profile:DirectX_11

mgfxc ..\Techniques.PBR.fx MetallicRoughnessEffect.ogl.mgfxo /Defines:MATERIAL_METALLICROUGHNESS
mgfxc ..\Techniques.PBR.fx MetallicRoughnessEffect.dx11.mgfxo /Defines:MATERIAL_METALLICROUGHNESS /Profile:DirectX_11

mgfxc ..\Techniques.PBR.fx SpecularGlossinessEffect.ogl.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS
mgfxc ..\Techniques.PBR.fx SpecularGlossinessEffect.dx11.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS /Profile:DirectX_11

endlocal
pause