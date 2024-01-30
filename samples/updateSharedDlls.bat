set "inputDir=.\CommandDispatcher.Mqtt.Dispatcher.ConsoleHost\bin\Debug\net8.0"
set "outputDir=.\CommandDispatcher.Libraries"

@REM Uncomment these lines if referencing from the post-build event in ConsoleHost.proj
@REM set "inputDir=.\bin\Debug\net7.0"
@REM set "outputDir=..\Samples\CommandDispatcher.Libraries"

if not exist %outputDir% mkdir %outputDir%

copy %inputDir%\CommandDispatcher.Mqtt.Interfaces.dll %outputDir%\CommandDispatcher.Mqtt.Interfaces.dll
copy %inputDir%\CommandDispatcher.Mqtt.Core.dll %outputDir%\CommandDispatcher.Mqtt.Core.dll
