# IMV Messaging Samples

## Overview

This sample demonstrates how the CommandDispatcher, hosted via the Dispatcher.ConsoleHost, is used as a stand-alone service to route messages to workloads that cannot directly interact with AIO MQ. The sample consists of the InMemoryHttpDeviceRegistry process and the DeviceRegistryCommandRouters that interact with the InMemoryHttpDeviceRegistry's HTTP API. The DeviceRegistryCommandRouters project implement the IRegisterCommandRouter and ICommandRouter interfaces provided by the CommandDispatcher.Interfaces library. The implemented message routers are loaded into the ConsoleHost using the [C# plugin model](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support), which then subscribes to the appropriate topics and forwards messages to the appropriate CommandRouter.

The CommandRouters are responsible for taking the incoming messages and using the data to make the appropriate HTTP calls to the DeviceRegistry service. The HTTP response is received by the CommandRouter and transformed into an MQTT message and published onto the appropriate topic.

## Components

- CommandDispatcher.Mqtt.Dispatcher.ConsoleHost: This runs as a stand-along process that loads the CommandRouters and subscribes to the appropriate topics.
- InMemoryHttpDeviceRegistry: A simple Web API that allows inserting devices and retrieving them all in a list or individually by Id. It uses an in-memory database supplied by EntityFramework.
- DeviceRegistryCommandRouters: There are three message routers, one that handles the POST and two for the GETs. There is a different message type for each as seen in the MessageSelector.

## Pre-requisites

The following are required to run the DeviceRegistry sample:
    - A running MQTT broker such as AIO MQ or Mosquitto
    - An MQTT tool such as the mosquitto client command line utilities (i.e. mosquitto_sub and mosquitto_pub) or MQTT Explorer that will enable viewing topics and publishing messages.

## Execution

To run the sample, execute these steps in the following order:

1. Build the CommandDispatcher.Mqtt.Dispatcher.ConsoleHost project.
2. Run the script appropriate to your operating system to copy the CommandDispatcher.Mqtt.Interfaces and CommandDispatcher.Mqtt.Models to the /Samples/SharedLibraries folder. The DeviceRegistryCommandRouters project requires those DLLs to build.
   - Windows: \Samples\updateSharedDlls.bat
   - Linux: /Samples/updateSharedDlls.sh
3. Build the DeviceRegistryCommandRouter projects.
4. Run the script appropriate to your operating system to copy the appropriate DLLs from the DeviceRegistryCommandRouter project to the /Samples/PluginLibraries folder.
   - Windows: \Samples\updateDeviceRegistryCommandRouterDlls.bat
   - Linux: /Samples/updateDeviceRegistryCommandRouterDlls.sh
5. Update (or add where missing) the following nodes in the CommandDispatcher.Mqtt.ConsoleHost appsettings.json:

   ```json
        "MqttSettings": {
            "ServerAddress": "localhost",
            "ServerPort": 1883,
            "MqttQualityOfServiceLevel": 1
        },
        "CommandRouterDlls": [
                "C:\\Dev\\IndustrialMetaverse\\ImvSoftwareDelivery\\src\\CommandDispatcher\\Samples\\PluginLibraries\\DeviceRegistryCommandRouters.dll"
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
    - DeviceRegistry: These settings will be used by the InMemoryHttpDeviceRegistry and DeviceRegistryCommandRouters.
      - BaseAddress based needs to match the InMemoryHttpDeviceRegistry API address.
      - ApiEndpoint should not change unless the corresponding changes are made to the DeviceRegistry project.
      - IncomingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to subscribe to the appropriate messages.
      - OutgoingTopic is the MQTT Topic used by the DeviceRegistryCommandRouters to publish responses.
6. Run the InMemoryHttpDeviceRegistry process to start the Web API
7. Run the CommandDispatcher.MqttDispatcher.ConsoleHost
   - This will load the DeviceRegistryCommandRouters.dll into the ConsoleHost process and subscribe to the IncomingTopic defined in appsettings.json.
8. Using your preferred MQTT tool, subscribe to the OutputTopic if necessary (e.g., mosquitto_sub), and start by populating the device registry with devices by sending a series of CreateDevice messages:

    ```bash
        cat sampleCreateDeviceMessages.json | mosquitto_pub -t device-registry/incoming -l
    ```

9. You should have seen the confirmation responses in the OutgoingTopic with 10 DeviceIds ranging from 'Device001' to 'Device010'.
10. You can now send messages to select a single device or list all devices using the following commands:

    ```bash
        mosquitto_pub -t device-registry/incoming -m '{"id":"b968686b979040a19e38e55280cf56bc", "correlationId":"180dca13-81b7-49e5-a081-9bf47be645ec", "contextId":"71df678b-fec8-46c0-ad2d-585116067177", "messageType":"GetDevice", "payload":"1", "createdAt":"2023-12-04T07:27:50.351065Z"}'

        mosquitto_pub -t device-registry/incoming -m '{"id":"b968686b979040a19e38e55280cf56bc", "correlationId":"180dca13-81b7-49e5-a081-9bf47be645ec", "contextId":"71df678b-fec8-46c0-ad2d-585116067177", "messageType":"GetAllDevices", "payload":"", "createdAt":"2023-12-04T07:27:50.351065Z"}'

    ```
11. You should see the responses in the OutgoingTopic.