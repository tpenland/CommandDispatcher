﻿using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommandDispatcher.Mqtt.Core
{
    public class CommandDispatcher<T>
    {
        private readonly ILogger<CommandDispatcher<T>> _logger;
        private readonly Dictionary<string, IList<ICommandRouter<T>>> _CommandRoutersByTopic = [];

        public CommandDispatcher(IPubSubClient<T> client, IEnumerable<ICommandRouter<T>> CommandRouters, ILogger<CommandDispatcher<T>> logger)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(CommandRouters);
            if (!CommandRouters.Any())
            {
                throw new ArgumentException("No CommandRouters passed into CommandDispatcher");
            }

            _logger = logger;
            foreach (ICommandRouter<T> router in CommandRouters)
            {
                router.PubSubClient = client;
                if (_CommandRoutersByTopic.TryGetValue(router.IncomingTopic, out IList<ICommandRouter<T>>? routers))
                {
                    routers.Add(router);
                }
                else
                {
                    _CommandRoutersByTopic[router.IncomingTopic] = [router];
                }
            }

            foreach (string topic in _CommandRoutersByTopic.Keys)
            {
                _logger.LogInformation("Subscribing to topic {topic}", topic);
                client.Subscribe(topic, DispatchMessage).Wait();
            }
        }

        private async Task DispatchMessage(string topic, T message)
        {
            var routersForTopic = _CommandRoutersByTopic[topic];

            foreach (ICommandRouter<T> CommandRouter in routersForTopic.Where(CommandRouter => CommandRouter.MessageSelector(message)))
            {
                _logger?.LogDebug("Forwarding message to {CommandRouter}", CommandRouter.GetType().Name);
                await CommandRouter.RouteAsync(message);
            }
        }
    }
}
