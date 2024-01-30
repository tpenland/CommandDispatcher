#!/bin/bash

inputDir=./DeviceRegistryCommandRouters/bin/Debug/net7.0
outputDir=./PluginLibraries

cp $inputDir/DeviceRegistryCommandRouters.dll $outputDir/DeviceRegistryCommandRouters.dll
cp $inputDir/DeviceRegistryCommandRouters.pdb $outputDir/DeviceRegistryCommandRouters.pdb
cp $inputDir/DeviceRegistryCommandRouters.deps.json $outputDir/DeviceRegistryCommandRouters.deps.json
cp $inputDir/Microsoft.Extensions.Http.dll $outputDir/Microsoft.Extensions.Http.dll