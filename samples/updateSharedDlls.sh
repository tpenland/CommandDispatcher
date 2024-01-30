#!/bin/bash

inputDir=../CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/bin/Debug/net8.0
outputDir=./CommandDispatcher.Libraries

if [ ! -d "$outputDir" ]; then
  mkdir -p $outputDir
fi

cp $inputDir/CommandDispatcher.Mqtt.Interfaces.dll $outputDir/CommandDispatcher.Mqtt.Interfaces.dll
cp $inputDir/CommandDispatcher.Mqtt.Core.dll $outputDir/CommandDispatcher.Mqtt.Core.dll