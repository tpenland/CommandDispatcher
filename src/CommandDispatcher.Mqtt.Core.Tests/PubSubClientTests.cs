// Ignore Spelling: Unsubscribe

using Azure.Messaging;
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

        [Fact]
        public async Task IncorrectMqttSettings_NonGeneric_ThrowsException()
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1884,
                MqttQualityOfServiceLevel = 0
            };

            var logger = new Mock<ILogger<PubSubClient>>();
            var client = new PubSubClient(mqttSettings, logger.Object);
            var ex = default(Exception);
            var eventReceived = new ManualResetEvent(false);
            client.ConnectingFailed += (args) => 
            { 
                ex = args.Exception; 
                eventReceived.Set();
                return Task.CompletedTask;
            };
            await client.Publish(GenerateRandomTopicName(), "Bird is the word.");
            eventReceived.WaitOne(5000);
            Assert.NotNull(ex);
            Assert.Contains("1884", ex.Message);
            Assert.Contains("localhost", ex.Message);
        }

        [Fact]
        public async Task IncorrectMqttSettings_Generic_ThrowsException()
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1884,
            };

            var logger = new Mock<ILogger<PubSubClient<CloudEvent>>>();
            var client = new PubSubClient<CloudEvent>(mqttSettings, logger.Object);
            var ex = default(Exception);
            var eventReceived = new ManualResetEvent(false);
            client.ConnectingFailed += (args) =>
            {
                ex = args.Exception;
                eventReceived.Set();
                return Task.CompletedTask;
            };
            await client.Publish(GenerateRandomTopicName(), new CloudEventGenerator().Generate(1));
            eventReceived.WaitOne(5000);
            Assert.NotNull(ex);
            Assert.Contains("1884", ex.Message);
            Assert.Contains("localhost", ex.Message);
        }

        [Theory]
        [ClassData(typeof(CloudEventGenerator))]
        public async Task Publish_CloudEvents_SendCorrectlyTest(CloudEvent testMessage)
        {
            string topic = GenerateRandomTopicName();

            var pubSubClient = GetTestPubSubClientForCloudEvents();
            var evalPubSubClient = GetTestPubSubClientForCloudEvents();

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

            var pubSubClient = GetTestPubSubClient();

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

        [Fact]
        public async Task Publish_RetainedMessage_IsRetained()
        {
            var testTopic = GenerateRandomTopicName();

            var pubSubClient = GetTestPubSubClientForCloudEvents();
            var testMessage = new CloudEventGenerator().Generate(1);
            testMessage.SetCorrelationId(null);
            await pubSubClient.Publish(testTopic, testMessage, true);
            await Task.Delay(1000);

            var receivedMessage = new ManualResetEvent(false);
            CloudEvent? messageReceived = default;
            await pubSubClient.Subscribe(testTopic, (string topic, CloudEvent message) =>
            {
                messageReceived = message;
                receivedMessage.Set();
                return Task.CompletedTask;
            });

            receivedMessage.WaitOne(1000);

            Assert.True(receivedMessage != default);
            Assert.True(new CloudEventEqualityComparer().Equals(testMessage, messageReceived), $"Expected {testMessage?.ToString()} but received {messageReceived}");
        }

        [Fact]
        public async Task ClearRetainedMessage_Generic_SuccessfullyClearsMessage()
        {
            var testTopic = GenerateRandomTopicName();

            var pubSubClient = GetTestPubSubClientForCloudEvents();
            var testMessage = new CloudEventGenerator().Generate(1);
            await pubSubClient.Publish(testTopic, testMessage, true);
            await Task.Delay(1000);

            await pubSubClient.ClearRetainedMessage(testTopic);

            var receivedMessage = new ManualResetEvent(false);
            CloudEvent? messageReceived = default;
            await pubSubClient.Subscribe(testTopic, (string topic, CloudEvent message) =>
            {
                messageReceived = message;
                receivedMessage.Set();
                return Task.CompletedTask;
            });

            receivedMessage.WaitOne(1000);

            Assert.Equal(default, messageReceived);
        }

        [Fact]
        public async Task ClearRetainedMessage_NonGeneric_SuccessfullyClearsMessage()
        {
            var testTopic = GenerateRandomTopicName();

            var pubSubClient = GetTestPubSubClient();
            var testMessage = "Important message";
            await pubSubClient.Publish(testTopic, testMessage, true);
            await Task.Delay(1000);

            await pubSubClient.ClearRetainedMessage(testTopic);

            var receivedMessage = new ManualResetEvent(false);
            string? messageReceived = default;
            await pubSubClient.Subscribe(testTopic, (string topic, string message) =>
            {
                messageReceived = message;
                receivedMessage.Set();
                return Task.CompletedTask;
            });

            receivedMessage.WaitOne(1000);

            Assert.Equal(default, messageReceived);
        }

        [Fact]
        public async Task Unsubscribe_NonGeneric_DoesNotGetMessage()
        {
            string topic = GenerateRandomTopicName();
            var testMessage = new CloudEventGenerator().Generate(1);

            var pubSubClient = GetTestPubSubClientForCloudEvents();
            var evalPubSubClient = GetTestPubSubClientForCloudEvents();

            var messageReceived = new ManualResetEvent(false);
            CloudEvent? receivedMessage1 = default;
            await evalPubSubClient.Subscribe(topic, (string topic, CloudEvent message) =>
            {
                receivedMessage1 = message;
                messageReceived.Set();
                return Task.CompletedTask;
            });

            await Task.Delay(1000);

            await pubSubClient.Publish(topic, testMessage);

            messageReceived.WaitOne(1000);

            messageReceived.Reset();

            CloudEvent? receivedMessage2 = default;
            await pubSubClient.UnSubscribe(topic);
            await pubSubClient.Publish(topic, testMessage);
            await Task.Delay(1000);

            Assert.True(receivedMessage1 != default);
            Assert.True(new CloudEventEqualityComparer().Equals(testMessage, receivedMessage1), $"Expected {testMessage?.ToString()} but received {receivedMessage1}");
            Assert.True(receivedMessage2 == default);
        }

        [Fact]
        public async Task Unsubscribe_Generic_DoesNotGetMessage()
        {
            string topic = GenerateRandomTopicName();
            var testMessage = "Test message";

            var pubSubClient = GetTestPubSubClient();
            var evalPubSubClient = GetTestPubSubClient();

            var messageReceived = new ManualResetEvent(false);
            string? receivedMessage1 = default;
            await evalPubSubClient.Subscribe(topic, (string topic, string message) =>
            {
                receivedMessage1 = message;
                messageReceived.Set();
                return Task.CompletedTask;
            });

            await Task.Delay(1000);

            await pubSubClient.Publish(topic, testMessage);

            messageReceived.WaitOne(1000);

            messageReceived.Reset();

            string? receivedMessage2 = default;
            await pubSubClient.UnSubscribe(topic);
            await pubSubClient.Publish(topic, testMessage);
            await Task.Delay(1000);

            Assert.True(receivedMessage1 != default);
            Assert.Equal(testMessage, receivedMessage1);
            Assert.True(receivedMessage2 == default);
        }

        [Fact]
        public async Task PubSubClient_Dispose_CleansUp()
        {
            var pubSubClient = GetTestPubSubClientForCloudEvents();

            pubSubClient.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await pubSubClient.Publish(GenerateRandomTopicName(), new CloudEventGenerator().Generate(1)));
        }

        public static string GenerateRandomTopicName(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        public PubSubClient<CloudEvent> GetTestPubSubClientForCloudEvents()
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1883,
            };

            var logger = new Mock<ILogger<PubSubClient<CloudEvent>>>();
            return new PubSubClient<CloudEvent>(mqttSettings, logger.Object);
        }

        public PubSubClient GetTestPubSubClient()
        {
            var mqttSettings = new MqttSettings
            {
                ServerAddress = "localhost",
                ServerPort = 1883,
            };

            var logger = new Mock<ILogger<PubSubClient>>();
            return new PubSubClient(mqttSettings, logger.Object);
        }
    }
}