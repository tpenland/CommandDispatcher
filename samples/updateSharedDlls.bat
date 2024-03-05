set "inputDir=..\src\CommandDispatcher.Mqtt.Dispatcher.ConsoleHost\bin\Debug\net8.0"
set "outputDir=.\SharedLibraries"

@REM Uncomment these lines if referencing from the post-build event in ConsoleHost.proj
@REM set "inputDir=.\bin\Debug\net8.0"
@REM set "outputDir=..\Samples\SharedLibraries"

if not exist %outputDir% mkdir %outputDir%

copy %inputDir%\CommandDispatcher.Mqtt.Interfaces.dll %outputDir%\CommandDispatcher.Mqtt.Interfaces.dll
copy %inputDir%\CommandDispatcher.Mqtt.Core.dll %outputDir%\CommandDispatcher.Mqtt.Core.dll
copy %inputDir%\CommandDispatcher.Mqtt.CloudEvents.dll %outputDir%\CommandDispatcher.Mqtt.CloudEvents.dll