@echo off
setlocal

SET MGFXC="C:\%HOMEPATH%\.nuget\packages\dotnet-mgcb\3.8.0.1375-develop\tools\netcoreapp3.1\any\mgfxc.dll"


del MetallicRoughnessEffect.*.mgfxo

dotnet %MGFXC% ..\PBR.fx MetallicRoughnessEffect.ogl.mgfxo /Debug /Defines:MATERIAL_METALLICROUGHNESS
dotnet %MGFXC% ..\PBR.fx MetallicRoughnessEffect.dx11.mgfxo /Profile:DirectX_11 /Debug /Defines:MATERIAL_METALLICROUGHNESS

dotnet %MGFXC% ..\PBR.fx SpecularGlossinessEffect.ogl.mgfxo /Debug /Defines:MATERIAL_SPECULARGLOSSINESS
dotnet %MGFXC% ..\PBR.fx SpecularGlossinessEffect.dx11.mgfxo /Profile:DirectX_11 /Debug /Defines:MATERIAL_SPECULARGLOSSINESS

endlocal
pause