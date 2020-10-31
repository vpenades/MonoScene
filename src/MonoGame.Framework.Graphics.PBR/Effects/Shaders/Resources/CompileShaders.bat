@echo off
setlocal

rem install https://www.nuget.org/packages/dotnet-mgfxc/

del *.mgfxo

mgfxc ..\Techniques.Unlit.Old.fx Unlit.ogl.mgfxo
mgfxc ..\Techniques.Unlit.Old.fx Unlit.dx11.mgfxo /Profile:DirectX_11

mgfxc ..\Techniques.PBR.Old.fx MetallicRoughnessEffect.ogl.mgfxo /Defines:MATERIAL_METALLICROUGHNESS
mgfxc ..\Techniques.PBR.Old.fx MetallicRoughnessEffect.dx11.mgfxo /Defines:MATERIAL_METALLICROUGHNESS /Profile:DirectX_11

mgfxc ..\Techniques.PBR.Old.fx SpecularGlossinessEffect.ogl.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS
mgfxc ..\Techniques.PBR.Old.fx SpecularGlossinessEffect.dx11.mgfxo /Defines:MATERIAL_SPECULARGLOSSINESS /Profile:DirectX_11

endlocal
pause