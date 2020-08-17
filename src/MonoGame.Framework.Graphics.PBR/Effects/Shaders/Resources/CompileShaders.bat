@echo off
setlocal

SET MGFXC="C:\%HOMEPATH%\.nuget\packages\dotnet-mgcb\3.8.0.1375-develop\tools\netcoreapp3.1\any\mgfxc.dll"


del *.mgfxo

dotnet %MGFXC% ..\Unlit.Permutations.fx Unlit.ogl.mgfxo /Debug
dotnet %MGFXC% ..\Unlit.Permutations.fx Unlit.dx11.mgfxo /Profile:DirectX_11

dotnet %MGFXC% ..\PBR.Permutations.fx MetallicRoughnessEffect.ogl.mgfxo /Debug /Defines:MATERIAL_METALLICROUGHNESS
dotnet %MGFXC% ..\PBR.Permutations.fx MetallicRoughnessEffect.dx11.mgfxo /Profile:DirectX_11 /Defines:MATERIAL_METALLICROUGHNESS

dotnet %MGFXC% ..\PBR.Permutations.fx SpecularGlossinessEffect.ogl.mgfxo /Debug /Defines:MATERIAL_SPECULARGLOSSINESS
dotnet %MGFXC% ..\PBR.Permutations.fx SpecularGlossinessEffect.dx11.mgfxo /Profile:DirectX_11 /Defines:MATERIAL_SPECULARGLOSSINESS

endlocal
pause