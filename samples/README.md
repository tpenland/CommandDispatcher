# IMV Messaging Samples

## Overview

This samples folder contains three different executable projects used to demonstrate the capabilities of the CommandDispatcher. The [DeviceRegistryHttp](./DeviceRegistryHttp/) and [DeviceRegistryMqtt](./DeviceRegistryMqtt/) projects each implement the same simple functionality to create, query and delete devices using an in-memory database. The former does so via an HTTP based WebAPI and the latter via subscribing and publishing directly to an MQTT broker. In addition, there is a [DeviceRegistryTestClient](./DeviceRegistryTestClient/) that can be used to create, query and delete devices by publishing MQTT messages and subscribing to the results.

These can be used to demonstrate the two primary runtime models for the CommandDispatcher:

1. <a id="Hosted"></a>Hosted - Using the [Dispatcher.ConsoleHost](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/), the CommandDispatcher is used as a stand-alone service to route messages to workloads that cannot directly interact with AIO MQ. The sample process [DeviceRegistryHttp](./DeviceRegistryHttp/) runs as a WebAPI and the [DeviceRegistryHttpCommandRouters](./DeviceRegistryHttpCommandRouters/) are loaded into the Dispatcher.Console host at runtime to interact with the DeviceRegistryHttp's HTTP API. The ConsoleHost subscribes to the appropriate MQTT topics based on the implemented CommandRouters and supplied configuration, which in turn make the appropriate calls to the Web API, receive the results and publish the responses to the appropriate topic.
1. <a id="Embedded"></a>Embedded - [DeviceRegistryMqtt](./DeviceRegistryMqtt/) demonstrates how to use the CommandDispatcher within an application to interact with MQTT and receive commands. It takes a direct dependency on the CommandDispatcher and uses it internally to subscribe and publish directly to an MQTT broker.

## Components

- [DeviceRegistryHttpCommandRouters](./DeviceRegistryHttpCommandRouters/): A library that implements the [IRegisterCommandRouters](../src/CommandDispatcher.Mqtt.Interfaces/IRegisterCommandRouters.cs) and [ICommandRouter](../src/CommandDispatcher.Mqtt.Interfaces/ICommandRouter.cs) interfaces provided by the [CommandDispatcher.Interfaces](../src/CommandDispatcher.Mqtt.Interfaces/) library. Each command router is responsible for taking the appropriate incoming messages and using the data to make the appropriate HTTP calls to the DeviceRegistry API. The HTTP response is received by the CommandRouter and transformed into an MQTT message and published onto the appropriate topic. There are command routers for performing HTTP actions for POST to create devices, GETs to retrieve them and a DELETE. There is a different command type for each as seen in the MessageSelector.
- [DeviceRegistryHttp](./DeviceRegistryHttp/): A simple Web API that allows inserting, retrieving and deleting devices. It uses an in-memory database supplied by EntityFramework.
- [DeviceRegistryMqtt](./DeviceRegistryMqtt/): A console application that subscribes to messages on the configured topic, performing device creates, queries and deletes as per the incoming messages. Responses are posted on the configured response topic.
- [DeviceRegistryTestClient](./DeviceRegistryTestClient/): A console app used to test the CommandDispatcher used in either the Hosted or Embedded examples. It publishes create, query and delete device messages to the configured topic, and subscribes to the configured response topic, printing out any received message.
- [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/): This runs as a stand-along process that loads the CommandRouters and subscribes to the appropriate topics. The implemented message routers are loaded into the ConsoleHost using the [C# plugin model](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support).

## Pre-requisites

The following are required to run the DeviceRegistry sample:
    - A running MQTT broker such as AIO MQ or Mosquitto
    - An MQTT tool such as the mosquitto client command line utilities (i.e. mosquitto_sub and mosquitto_pub) or MQTT Explorer that will enable viewing topics and publishing messages.

## Execution

### [Embedded](#Embedded) Sample

1. If the [SharedLibraries](./SharedLibraries/) folder is empty (or if the DLLs need to be updated due to changes in the CommandDispatcher code), do the following:
   - Build the [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/) project.
   - Run the script appropriate to your operating system to copy the required DLLs (see CommandDispatcher.Mqtt.Dispatcher.ConsoleHost.deps.json) to the /Samples/SharedLibraries folder.
     - Windows: [updateSharedDlls.bat](./updateSharedDlls.bat)
     - Linux: [updateSharedDlls.sh](./updateSharedDlls.sh)
2. Update (or add where missing) the following nodes in the DeviceRegistryMqtt [appsettings.json](./DeviceRegistryMqtt/appsettings.json) with the appropriate values:

   ```json
        "MqttSettings": {
            "ServerAddress": "localhost",
            "ServerPort": 1883,
            "MqttQualityOfServiceLevel": 1
        },
        "DeviceRegistry": {
            "IncomingTopic": "device-registry/incoming",
            "OutgoingTopic": "device-registry/outgoing"
        }
   ```

    - MqttSettings: This should be modified to match address and port of the running MQTT broker.
    - DeviceRegistry: These settings will be used by the DeviceRegistryHttp and DeviceRegistryCommandRouters.
      - IncomingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to subscribe to the appropriate messages.
      - OutgoingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to publish responses.
3. Run the [DeviceRegistryMqtt](./DeviceRegistryMqtt/) project.
4. Update the DeviceRegistryTestClient [appsettings.json](./DeviceRegistryTestClient/appsettings.json) with the values used above in the DeviceRegistryMqtt [appsettings.json] to ensure the correct MQTT broker and topics are used by the test client.
5. Run the [DeviceRegistryTestClient].
   - Use the menu to automatically create, query and delete devices.
   - Output will be printed out to the console windows.
6. Optionally, using your preferred [MQTT tool](../tools/mqtt/README.md) subscribe to the InputTopic and OutputTopic to monitor the messages.

### [Hosted](#Hosted) Sample

1. Build the [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/) project.
2. Run the script appropriate to your operating system to copy the required DLLs (see CommandDispatcher.Mqtt.Dispatcher.ConsoleHost.deps.json) to the /Samples/SharedLibraries folder. The DeviceRegistryCommandRouters project requires those DLLs to build.
   - Windows: [updateSharedDlls.bat](./updateSharedDlls.bat)
   - Linux: [updateSharedDlls.sh](./updateSharedDlls.sh)
3. Build the [DeviceRegistryHttpCommandRouters](./DeviceRegistryHttpCommandRouters/) project.
4. Run the script appropriate to your operating system to copy the appropriate DLLs from the DeviceRegistryCommandRouter project to the /Samples/PluginLibraries folder.
   - Windows: [updateDeviceRegistryCommandRouterDlls.bat](./updateDeviceRegistryCommandRouterDlls.bat)
   - Linux: [updateDeviceRegistryCommandRouterDlls.sh](./updateDeviceRegistryCommandRouterDlls.sh)
5. Update (or add where missing) the following nodes in the CommandDispatcher.Mqtt.Dispatcher.ConsoleHost [appsettings.json](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/appsettings.json) with the appropriate values:

   ```json
        "MqttSettings": {
            "ServerAddress": "localhost",
            "ServerPort": 1883,
            "MqttQualityOfServiceLevel": 1
        },
        "CommandRouterDlls": [
                "C:\\Dev\\CommandDispatcher\\Samples\\PluginLibraries\\DeviceRegistryCommandRouters.dll"
            ],
        "DeviceRegistry": {
            "BaseAddress": "http://localhost:5270/",
            "ApiEndpoint": "devices/",
            "IncomingTopic": "device-registry/incoming",
            "OutgoingTopic": "device-registry/outgoing"
        }
   ```

    - MqttSettings: This should be modified to match address and port of the running MQTT broker.
    - CommandRouterDlls: This should point to folder containing the DeviceRegistryCommandRouters.dll (e.g. \Samples\PluginLibraries)
    - DeviceRegistry: These settings will be used by the DeviceRegistryHttp and DeviceRegistryCommandRouters.
      - BaseAddress based needs to match the InMemoryHttpDeviceRegistry API address.
      - ApiEndpoint should not change unless the corresponding changes are made to the DeviceRegistry project.
      - IncomingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to subscribe to the appropriate messages.
      - OutgoingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to publish responses.
6. Run the [DeviceRegistryHttp](./DeviceRegistryHttp/) process to start the Web API.
7. Run the [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](../src/CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/)
   - This will load the DeviceRegistryCommandRouters.dll into the ConsoleHost process and subscribe to the IncomingTopic defined in appsettings.json.
8. Update the DeviceRegistryTestClient [appsettings.json](./DeviceRegistryTestClient/appsettings.json) with the values used above in the CommandDispatcher.Mqtt.Dispatcher.ConsoleHost [appsettings.json] to ensure the correct MQTT broker and topics are used by the test client.
9. Run the [DeviceRegistryTestClient].
   - Use the menu to automatically create, query and delete devices.
   - Output will be printed out to the console windows.
10. Optionally, using your preferred [MQTT tool](../tools/mqtt/README.md) subscribe to the InputTopic and OutputTopic to monitor the messages.
