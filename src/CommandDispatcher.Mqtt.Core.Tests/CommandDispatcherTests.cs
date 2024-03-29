﻿using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;
using CommandDispatcher.Utilities;
using Microsoft.Extensions.Logging;
using Moq;

namespace CommandDispatcher.Mqtt.Core.Tests
{
    public class CommandDispatcherTests
    {
        const string commandTopic = "commands";
        const string responseTopic = "responses";

        public CommandDispatcherTests()
        {
            DockerHelper.Instance.StartMosquitto($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}mosquitto.conf");
        }

        [Fact]
        public async Task Subscribe_MultipleSelectors_SendProperResponse()
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1883,
            };
            var pubSubLogger = new Mock<ILogger<PubSubClient<CloudEvent>>>();
            var dispatcherLogger = new Mock<ILogger<CommandDispatcher<CloudEvent>>>();

            var routers = GetCommandRouters();
            var pubSubClient = new PubSubClient<CloudEvent>(mqttSettings, pubSubLogger.Object);
            _ = new CommandDispatcher<CloudEvent>(pubSubClient, routers, dispatcherLogger.Object);

            var evalPubSubClient = new PubSubClient<CloudEvent>(mqttSettings, pubSubLogger.Object);

            var testTopicAndMessageList = GetTestMessages();
            var messagesReceived = new List<CloudEvent>();

            await evalPubSubClient.Subscribe(responseTopic, (string topic, CloudEvent message) =>
            {
                Assert.False(message.Equals(default));
                messagesReceived.Add(message);
                return Task.CompletedTask;
            });

            await Task.Delay(1000);

            foreach ((var topic, var testMessage) in testTopicAndMessageList)
            {
                await pubSubClient.Publish(topic, testMessage);
            }

            SpinWait.SpinUntil(() => messagesReceived.Count == 6, 5000);

            Assert.Equal(6, messagesReceived.Count);
            foreach (var topicMessageTuple in testTopicAndMessageList)
            {
                var responses = messagesReceived.Where(m => m.GetCorrelationId() == topicMessageTuple.Item2.GetCorrelationId());
                foreach (var response in responses)
                {
                    Assert.True(response.Data!.ToString() == topicMessageTuple.Item2.Data!.ToString(),
                        $"Expected data {topicMessageTuple.Item2.Data} but received {response.Data}");
                }
            }
            var correlationCount = messagesReceived.GroupBy(messagesReceived => messagesReceived.GetCorrelationId())
                .Select(grp => new { CorrelationId = grp.Key, Count = grp.Count() })
                .Where(grp => grp.Count > 1)
                .ToList();

            Assert.True(correlationCount.Count == 1, "There should only be 1 repeated correlationId");
            var messageE = testTopicAndMessageList.First(m => m.Item2.Data!.ToString().Trim('"') == "E").Item2;
            Assert.True(correlationCount[0].CorrelationId == messageE.GetCorrelationId(), "Message with Data E should be the duplicated message.");
            Assert.IsType<string>(messageE.GetCorrelationIdType());
        }

        internal List<ICommandRouter<CloudEvent>> GetCommandRouters()
        {
            return
            [
                new CommandType01CommandRouter(),
                new CommandType02CommandRouter(),
                new DeviceCommandCommandRouter(),
                new TopicCommandCommandRouter()
            ];
        }

        internal List<(string, CloudEvent)> GetTestMessages()
        {
            var topicMessageList = new List<(string, CloudEvent)>
            {
                (
                    commandTopic, new CloudEvent("CommandDispatcherTests", "TestCommand01", "A")
                ),
                (
                    commandTopic, new CloudEvent("CommandDispatcherTests", "TestCommand02", "B")
                ),
                (
                    commandTopic, new CloudEvent("Device001", "TestCommand03", "C")
                ),
                (
                    $"{commandTopic}/foobar", new CloudEvent("Device002", "TestCommand04", "D")
                ),
                (
                    // This message will be duplicated between the CommandTypeCommandRouter and the DeviceCommandCommandRouter
                    commandTopic, new CloudEvent("Device001", "TestCommand02", "E")
                ),
            };
            topicMessageList.ForEach(m => m.Item2.SetCorrelationId(Guid.NewGuid().ToString()));
            return topicMessageList;
        }

        internal class CommandType01CommandRouter : ICommandRouter<CloudEvent>
        {
            public Predicate<CloudEvent> MessageSelector => message => message.Type == "TestCommand01";
            public string IncomingTopic => "commands";
            public string OutgoingTopic => "responses";
            public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

            public async Task RouteAsync(CloudEvent message)
            {
                // Simulate call to code that will handle the message.
                await Task.Delay(100);

                var response = new CloudEvent(message.Source, message.Type, message.Data, "text/plain");
                response.SetCorrelationId(message.GetCorrelationId());

                if (PubSubClient != null)
                {
                    await PubSubClient.Publish(OutgoingTopic, response);
                }
            }
        }

        internal class CommandType02CommandRouter : ICommandRouter<CloudEvent>
        {
            public Predicate<CloudEvent> MessageSelector => message => message.Type == "TestCommand02";
            public string IncomingTopic => "commands";
            public string OutgoingTopic => "responses";
            public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

            public async Task RouteAsync(CloudEvent message)
            {
                // Simulate call to code that will handle the message.
                await Task.Delay(100);
                
                var response = new CloudEvent(message.Source, message.Type, message.Data, "text/plain");
                response.SetCorrelationId(message.GetCorrelationId());

                if (PubSubClient != null)
                {
                    await PubSubClient.Publish(OutgoingTopic, response);
                }
            }
        }

        internal class DeviceCommandCommandRouter : ICommandRouter<CloudEvent>
        {
            public Predicate<CloudEvent> MessageSelector => message => message.Source == "Device001";
            public string IncomingTopic => "commands";
            public string OutgoingTopic => "responses";
            public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

            public async Task RouteAsync(CloudEvent message)
            {
                // Simulate call to code that will handle the message.
                await Task.Delay(100);

                var response = new CloudEvent(message.Source, message.Type, message.Data, "text/plain");
                response.SetCorrelationId(message.GetCorrelationId());

                if (PubSubClient != null)
                {
                    await PubSubClient.Publish(OutgoingTopic, response);
                }
            }
        }

        internal class TopicCommandCommandRouter : ICommandRouter<CloudEvent>
        {
            public Predicate<CloudEvent> MessageSelector => _ => true; // all messages on the topic
            public string IncomingTopic => "commands/foobar";
            public string OutgoingTopic => "responses";
            public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

            public async Task RouteAsync(CloudEvent message)
            {
                // Simulate call to code that will handle the message.
                await Task.Delay(100);

                var response = new CloudEvent(message.Source, message.Type, message.Data, "text/plain");
                response.SetCorrelationId(message.GetCorrelationId());

                if (PubSubClient != null)
                {
                    await PubSubClient.Publish(OutgoingTopic, response);
                }
            }
        }
    }
}