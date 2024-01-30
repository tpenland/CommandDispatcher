# Command Dispatcher

## Overview and Rationale

The Command Dispatcher libraries offer two core capabilities:

1. A wrapper over an MQTT library.
2. A routing mechanism for MQTT messages.

Why?

1. Why wrap an MQTT library? Why wrap 3rd party libraries at all?
   - Facade: Present a simple interface tailored to our scenarios.
     - This allows us to tailor the complexity of generic, general purpose libraries to the specificity of our specific scenarios. A facade allows us to hide that complexity and encode best practices, thereby helping developers fall into the pit of success.
   - Decorator: Add behavior to a class by wrapping instead of subclassing.
     - In addition to encoding best practices, a wrapper allows us to add business rules to ensure that they are consistently enforced in one place in the code. Details such as message envelopes, batching and compression, auditing, etc. can all be added to this wrapper library.

2. Why do I need a routing mechanism? Isn't the topic structure sufficient?

    To answer these questions, it is helpful to compare MQTT to the more widely understood HTTP model. In HTTP, a URL will route messages against a domain and resource structure to find the correct handler, typically in the form of a web API. But how do messages get routed to the correct part of the code for each message? Built into HTTP protocol is a specification of methods: GET, POST, PUT, etc. These are then mapped by developers in the web API to the appropriate code to handle that method:

    ``` psuedo-code
        webapp.MapGet("/myResource/{id}", GetResourceHandler)
        webapp.MapPost("/myResource/", PostResourceHandler)
        ...
    ```

    Comparing that to the MQTT protocol, the topic structure serves as a routing mechansim in much the same way as the URL does in HTTP. However, unlike HTTP there are no methods like GET, POST, etc. in MQTT. Once the message is pulled from the topic, we typically use attributes on the message envelope, such as message type, to determine which part of the system - which component or method - should recieve the method. This will then often be implemented in a switch/case statment type structure:

    ``` psuedo-code
    switch (message.CommandType)
        case CommandType.A:
            DoA(message);
        case CommandType.B:
            DoB(message):
        case ...
    ```

    This is brittle code that can be challenging and costly (particularly from a testing perspective) to change, and is an example of violating the [Open-Closed Principle (OCP)](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle). The routing mechanism proposed here is based on the [Strategy Pattern](https://en.wikipedia.org/wiki/Strategy_pattern#:~:text=In%20computer%20programming%2C%20the%20strategy,family%20of%20algorithms%20to%20use) to allow the extension of behavior (adding new messages) without having to change existing code.

3. Ok, but that just begs the question - if I control the topic structure and default MQTT behavior is to create queues on demand, why not just create as many topics as operations?

   The answer to this question lies in one of MQTT's greatest strenghts: loose coupling. By introducing a broker between the message publisher and message subscribers, we minimize the amount of shared information needed for them to communicate. Creating topics down to the operation level, similar to an API specification, has a number of problems:
   1. Exposes granular details of the API forcing clients to couple their code this structure.
   2. Subscribers would have to subscribe to each topic to avoid any routing behavior. Subscribing to a wildcard would defeat the purpose of the granular topics and require internal routing based on topic name.
   3. At scale this would result in a massive proliferation of topics.
   4. While it is the default behavior of most brokers to create topics on demand, in the context of the AIO architecture, neither Event Hubs nor Event Grid MQTT broker can support this kind of dynamic behavior. The proliferation of topics will ripple into these subsystes, requiring provisioning and security on each and every topic/hub.

   Ultimately, the best practices for topic naming will depend on the scenario, but at scale the most granular that we should aim for would be down to the leaf-node level. This means we would expect to see topics down to the deviceId or workloadId level. In highly dynamic scenarios, this may move up a level (e.g., a topic for all devices of a specific type) which would require routing based on attributes in the message to find the actual instance. This solution is meant for these scenarios.

## Components

- [CommandDispatcher.Core](./CommandDispatcher.Mqtt.Core/)
  - *PubSubClient* - Wraps the underlying MQTT client and exposes Publish and Subscribe mechanics.
  - *CommandDispatcher* - Takes a list of ICommandRouter objects, subscribes to their topics and routes messages to them.
- [CommandDispatcher.Interfaces](./CommandDispatcher.Mqtt.Interfaces/)
  - *ICommandRouter* - The primary interface that drives the CommandDispatcher. An implementor specifies the incoming copic, a message selector predicate for fine grained message selection and the execution method, RouteAsync, that takes the incoming message, makes whatever calls it needs and sends back any responses on the response topic.
  - *IRegisterCommandRouters* - This is used by the stand-alone ConsoleHost and the .Net plugin model for loading the implemented ICommandRouters used to interact with external services.
  - *IPubSubClient* - Abstraction used to interact with the underlying .Net library that directly talks to the broker over the MQTT protocol.
- [CommandDispatcher.Models](./CommandDispatcher.Mqtt.Models/)
  - *MessageEnvelope* - Optional Record Type intended as a common envelope used for all communication among servcies over MQTT.
- [CommandDispatcher.Mqtt.Dispatcher.ConsoleHost](./CommandDispatcher.Mqtt.Dispatcher.ConsoleHost/)
  - *LoopbackCommandRouter* - Used for validating that the ConsoleHost is running. Subscribes to the topic 'loopback/input' and returns a message on the topic 'loopback/output'.
  - *appSettings.json* - This needs to be modified by implementors of MessageRouting libraries to include library specific configuration. For more information see the [Sample](./Samples/README.md) and the implementation example in [DeviceRegistryCommandRouter](./Samples/DeviceRegistryCommandRouters/DeviceRegistryCommandRouter.cs) for how to access the configuration data.
- [MqttNet](https://github.com/dotnet/MQTTnet)
  - This is the underlying library used by CommandDispatcher.Mqtt to interact with the MQTT protocol. It is an MQTT v5 compliant library that is supported by the .NET Foundation and is effetively the default MQTT libarary for the .Net community. This library is abstracted away behind the *PubSubClient* and any futher capabilities that need to exposed from MqttNet will be done by mofifying the *PubSubClient*.

## Deployment Models

1. **Stand-alone service** using the *ConsoleHost* project. In this mode the service loads DLLs from disk that implement the ICommandRouter interface, subscribe to the appropriate topics and forward messages to the appropriate CommandRouter. The implementation details are demonstrated using the sample [DeviceRegistry](./Samples/DeviceRegistry/) and supporting [DeviceRegistryCommandRouter DLL](./Samples/DeviceRegistryCommandRouters/) as discussed in the [Samples README](./Samples/README.md).
2. **Integrated DLLs** in a custom service. The CommandDispatcher components run in the process of the custom service, subscribing and routing messages as appropriate within the custom service.
