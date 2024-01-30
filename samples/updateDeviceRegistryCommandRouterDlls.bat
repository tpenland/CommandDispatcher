set "inputDir=.\DeviceRegistryCommandRouters\bin\Debug\net7.0"
set "outputDir=.\PluginLibraries"

@REM Uncomment these lines if referencing from the post-build event in ConsoleHost.proj
@REM set "inputDir=.\bin\Debug\net7.0"
@REM set "outputDir=..\PluginLibraries"

copy %inputDir%\DeviceRegistryCommandRouters.dll %outputDir%\DeviceRegistryCommandRouters.dll
copy %inputDir%\DeviceRegistryCommandRouters.pdb %outputDir%\DeviceRegistryCommandRouters.pdb
copy %inputDir%\DeviceRegistryCommandRouters.deps.json %outputDir%\DeviceRegistryCommandRouters.deps.json
copy %inputDir%\Microsoft.Extensions.Http.dll %outputDir%\Microsoft.Extensions.Http.dll