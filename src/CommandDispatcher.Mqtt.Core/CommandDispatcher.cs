﻿using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Logging;


namespace CommandDispatcher.Mqtt.Core
{
    public class CommandDispatcher<T>
    {
        private readonly ILogger<CommandDispatcher<T>> _logger;
        private readonly Dictionary<string, IList<ICommandRouter<T>>> _CommandRoutersByTopic = new();

        public CommandDispatcher(IPubSubClient<T> client, IEnumerable<ICommandRouter<T>> CommandRouters, ILogger<CommandDispatcher<T>> logger)
        {
            Guard.IsNull(client, nameof(client));
            Guard.IsNull(CommandRouters, nameof(CommandRouters));
            Guard.IsEmpty(CommandRouters.ToList(), nameof(CommandRouters));
            Guard.IsNull(logger, nameof(logger));

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
                    _CommandRoutersByTopic[router.IncomingTopic] = new List<ICommandRouter<T>> { router };
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
