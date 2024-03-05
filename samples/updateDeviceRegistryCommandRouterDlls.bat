set "inputDir=.\DeviceRegistryHttpCommandRouters\bin\Debug\net8.0"
set "outputDir=.\PluginLibraries"

@REM Uncomment these lines if referencing from the post-build event in ConsoleHost.proj
@REM set "inputDir=.\bin\Debug\net8.0"
@REM set "outputDir=..\PluginLibraries"

copy %inputDir%\DeviceRegistryHttpCommandRouters.dll %outputDir%\DeviceRegistryCommandRouters.dll
copy %inputDir%\DeviceRegistryHttpCommandRouters.pdb %outputDir%\DeviceRegistryCommandRouters.pdb
copy %inputDir%\DeviceRegistryHttpCommandRouters.deps.json %outputDir%\DeviceRegistryCommandRouters.deps.json
copy %inputDir%\Microsoft.Extensions.Http.dll %outputDir%\Microsoft.Extensions.Http.dll