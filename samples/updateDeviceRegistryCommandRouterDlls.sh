#!/bin/bash

inputDir=./DeviceRegistryHttpCommandRouters/bin/Debug/net8.0
outputDir=./PluginLibraries

cp $inputDir/DeviceRegistryHttpCommandRouters.dll $outputDir/DeviceRegistryCommandRouters.dll
cp $inputDir/DeviceRegistryHttpCommandRouters.pdb $outputDir/DeviceRegistryCommandRouters.pdb
cp $inputDir/DeviceRegistryHttpCommandRouters.deps.json $outputDir/DeviceRegistryCommandRouters.deps.json
cp $inputDir/Microsoft.Extensions.Http.dll $outputDir/Microsoft.Extensions.Http.dll