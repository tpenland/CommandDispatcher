# Command Dispatcher

## Overview

This library is a code accelerator helping custom workloads at the edge handle the expected synchronous request/response pattern of executing commands and sending responses when using the asynchronous publish/subscribe pattern of MQTT. More specifically, it is meant to help complex workloads that receive n number of different commands on one or more incoming topics properly route each command to the appropriate command handler, and return one or more responses back to the appropriate outbound topic. It has the following features:

1. Simple interface-based routing mechanism using a variation of the [strategy pattern](https://en.wikipedia.org/wiki/Strategy_pattern#:~:text=In%20computer%20programming%2C%20the%20strategy,family%20of%20algorithms%20to%20use).
   1. Message routers can define routing rules based on message attributes and include the ability to publish responses.
   2. New commands can be added without changing existing code.
2. Integrates the [CloudEvents](https://cloudevents.io/) specification using the [CloudEvents C# sdk](https://github.com/cloudevents/sdk-csharp?tab=readme-ov-file).
   1. Includes an implementation of [CorrelationId](./src/CommandDispatcher.Mqtt.CloudEvents/CorrelationId.cs) based on CloudEvents extension attribute guidance.
   2. Use of CloudEvents is entirely optional and any custom envelope or even no envelope can be used instead.
3. Includes an optional console host that allows the CommandDispatcher to be run as a stand-alone process acting as a gateway for other processes.
   1. Uses the [C# plugin model](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support) to enable adding message handlers from other processes without altering any code in the console host.
   2. Simply implement the appropriate interfaces (ICommandRouter and IRegisterCommandRouter) in a dll, drop it in the appropriate folder and add the dll name to the manifest.
4. Uses the MqttNet library for all Mqtt communication.
   1. Contains a facade, [PubSubClient](./src/CommandDispatcher.Mqtt.Core/PubSubClient.cs), used by the [CommandDispatcher](./src/CommandDispatcher.Mqtt.Core/CommandDispatcher.cs), that exposes simple publish and subscribe functionality.
   2. The PubSubClient class can be used and/or extended, or substituted with an alternative implementation of [IPubSubClient](./src/CommandDispatcher.Mqtt.Interfaces/IPubSubClient.cs). 
5. [Samples](./samples/README.md) folder that demonstrate the library's use:
   1. For an example of implementing the library as embedded dlls see
   2. For an example using the stand-alone console host versions see

## Design Considerations

To better understand the problem this library is meant to solve, let's compare and contrast handling commands using a typical HTTP web api versus using an MQTT broker. In HTTP, message routing and handling are well established patterns defined by the protocol. A URL, composed of a domain ('www.myCompany.com/api') and resource structure ("/myResource/{id}") will deliver the message to that endpoint, and then call the appropriate handler based on the built in methods GET, POST, PUT, etc. These are then mapped by developers in the web API to the appropriate code to handle that method:

    ``` psuedo-code
        webapp.MapGet("/myResource/{id}", GetResourceHandler)
        webapp.MapPost("/myResource/", PostResourceHandler)
        ...
    ```

Comparing that to the MQTT protocol, the topic structure serves as a routing mechanism in much the same way as the URL does in HTTP. However, unlike HTTP there are no built-in methods like GET, POST, etc. in MQTT. Once the message is pulled from the topic, we typically use attributes on the message envelope, such as message type, to determine which part of the system - which component or method - should receive the method. This will then often be implemented in a switch/case statement type structure:

    ``` psuedo-code
    switch (message.CommandType)
        case CommandType.A:
            DoA(message);
        case CommandType.B:
            DoB(message):
        case ...
    ```

This is brittle code that can be challenging and costly (particularly from a testing perspective) to change, and is an example of violating the [Open-Closed Principle (OCP)](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle). The routing mechanism proposed here is based on the [Strategy Pattern](https://en.wikipedia.org/wiki/Strategy_pattern#:~:text=In%20computer%20programming%2C%20the%20strategy,family%20of%20algorithms%20to%20use) to allow the extension of behavior (adding new messages) without having to change existing code.

## Implementation Overview

Using this library is straightforward and requires only a few steps:

- Implement the ICommandRouter interface for each command type.
- In your startup code, do the following:
  - Register your ICommandRouter implementations with your IoC mechanism.
  - Register the included PubSubClient, or your implementation of the IPubSubClient interface, with your IoC.
  - Register the CommandDispatcher and request an instance from your IoC. Note this instance can be requested on a background thread if the larger service is handling requests or running operations other than processing MQTT messages.

For further details and examples, see the [samples readme](./samples/README.md).

## Components

- [CommandDispatcher.Core](./CommandDispatcher.Mqtt.Core/)
  - *PubSubClient* - Wraps the underlying MQTT client and exposes Publish and Subscribe mechanics.
  - *CommandDispatcher* - Takes a list of ICommandRouter objects, subscribes to their topics and routes messages to them.
- [CommandDispatcher.Interfaces](./CommandDispatcher.Mqtt.Interfaces/)
  - *ICommandRouter* - The primary interface that drives the CommandDispatcher. An implementor specifies the incoming topic, a message selector predicate for fine grained message selection and the execution method, RouteAsync, that takes the incoming message, makes whatever calls it needs and sends back any responses on the response topic.
  - *IRegisterCommandRouters* - This is used by the stand-alone ConsoleHost and the .Net plugin model for loading the implemented ICommandRouters used to interact with external services.
  - *IPubSubClient* - Abstraction used to interact with the underlying .Net library that directly talks to the broker over the MQTT protocol.
- [CommandDispatcher.Models](./CommandDispatcher.Mqtt.Models/)
  - *MessageEnvelope* - Optional Record Type intended as a common envelope used for all communication among services over MQTT.
- [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](./CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/)
  - *LoopbackCommandRouter* - Used for validating that the ConsoleHost is running. Subscribes to the topic 'loopback/input' and returns a message on the topic 'loopback/output'.
  - *appSettings.json* - This needs to be modified by implementors of MessageRouting libraries to include library specific configuration. For more information see the [Sample](./Samples/README.md) and the implementation example in [DeviceRegistryCommandRouter](./Samples/DeviceRegistryCommandRouters/DeviceRegistryCommandRouter.cs) for how to access the configuration data.
- [MqttNet](https://github.com/dotnet/MQTTnet)
  - This is the underlying library used by CommandDispatcher.Mqtt to interact with the MQTT protocol. It is an MQTT v5 compliant library that is supported by the .NET Foundation and is effetively the default MQTT libarary for the .Net community. This library is abstracted away behind the *PubSubClient* and any further capabilities that need to exposed from MqttNet will be done by mofifying the *PubSubClient*.

## Deployment Models

1. **Stand-alone service** using the *ConsoleHost* project. In this mode the service loads DLLs from disk that implement the ICommandRouter interface, subscribe to the appropriate topics and forward messages to the appropriate CommandRouter. The implementation details are demonstrated using the sample [DeviceRegistry](./Samples/DeviceRegistry/) and supporting [DeviceRegistryCommandRouter DLL](./Samples/DeviceRegistryCommandRouters/) as discussed in the [Samples README](./Samples/README.md).
2. **Integrated DLLs** in a custom service. The CommandDispatcher components run in the process of the custom service, subscribing and routing messages as appropriate within the custom service.
