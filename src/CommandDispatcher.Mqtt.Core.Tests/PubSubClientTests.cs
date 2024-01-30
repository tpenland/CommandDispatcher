using CloudNative.CloudEvents;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.TestHelpers;
using CommandDispatcher.Utilities;
using Microsoft.Extensions.Logging;
using Moq;

namespace CommandDispatcher.Mqtt.Core.Tests
{
    public class PubSubClientTests
    {
        public PubSubClientTests()
        {
            DockerHelper.Instance.StartMosquitto($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}mosquitto.conf");
        }

        [Theory]
        [ClassData(typeof(CloudEventGenerator))]
        public async Task Publish_CloudEvents_SendCorrectlyTest(CloudEvent testMessage)
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1883,
            };

            string topic = GenerateRandomTopicName();

            var logger = new Mock<ILogger<PubSubClient<CloudEvent>>>();
            var pubSubClient = new PubSubClient<CloudEvent>(mqttSettings, new CloudEvenMqttFormatter(), logger.Object);
            var evalPubSubClient = new PubSubClient<CloudEvent>(mqttSettings, new CloudEvenMqttFormatter(), logger.Object);

            var messageReceived = new ManualResetEvent(false);
            CloudEvent? receivedMessage = default;
            await evalPubSubClient.Subscribe(topic, (string topic, CloudEvent message) =>
            {
                receivedMessage = message;
                messageReceived.Set();
                return Task.CompletedTask;
            });

            // Wait to give MqttNet time to get the engine rolling. Otherwise, the publish won't happen or the subscribe delegate will not fire.
            // Note that Thread.Sleep() is flagged as a warning in unit tests and SpinWait.SpinOnce() does not work.
            await Task.Delay(1000);

            await pubSubClient.Publish(topic, testMessage);

            messageReceived.WaitOne(1000);

            Assert.True(receivedMessage != default);
            Assert.True(new CloudEventEqualityComparer().Equals(testMessage, receivedMessage), $"Expected {testMessage?.ToString()} but received {receivedMessage}");
        }

        [Fact]
        public async Task Publish_MultipleTopics_EachGetsCorrectMessageTest()
        {
            const string testMessage = "this is a message for topic 2";
            const string testTopic1 = "testtopic1";
            const string testTopic2 = "testtopic2";
            const string testTopic3 = "testtopic2/mytest";

            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1883,
            };

            var logger = new Mock<ILogger>();
            var pubSubClient = new PubSubClient(mqttSettings, logger.Object);

            string receivedOnTopic = string.Empty;

            var messageReceived1 = new ManualResetEvent(false);
            string? message1 = null;

            await pubSubClient.Subscribe(testTopic1, (string topic, string messageJson) =>
            {
                message1 = messageJson;
                receivedOnTopic = testTopic1;
                messageReceived1.Set();
                return Task.CompletedTask;
            });

            var messageReceived2 = new ManualResetEvent(false);
            string? message2 = null;
            await pubSubClient.Subscribe(testTopic2, (string topic, string messageJson) =>
            {
                message2 = messageJson;
                receivedOnTopic = testTopic2;
                messageReceived2.Set();
                return Task.CompletedTask;
            });

            var messageReceived3 = new ManualResetEvent(false);
            string? message3 = null;
            await pubSubClient.Subscribe(testTopic3, (string topic, string messageJson) =>
            {
                message3 = messageJson;
                receivedOnTopic = testTopic3;
                messageReceived3.Set();
                return Task.CompletedTask;
            });

            // Wait to give MqttNet time to get the engine rolling. Otherwise, the publish won't happen or the subscribe delegate will.
            // Note that Thread.Sleep() is flagged as a warning in unit tests and SpinWait.SpinOnce() does not work.
            await Task.Delay(1000);

            await pubSubClient.Publish(testTopic2, testMessage);

            WaitHandle.WaitAny([messageReceived1, messageReceived2, messageReceived3], 1000);

            Assert.Null(message1);
            Assert.Null(message3);
            Assert.NotNull(message2);
            Assert.Equal(testTopic2, receivedOnTopic);
        }

        public static string GenerateRandomTopicName(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
    }
}