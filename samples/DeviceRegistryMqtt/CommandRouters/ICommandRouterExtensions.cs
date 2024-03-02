using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;

namespace DeviceRegistryMqtt.CommandRouters
{
    /// <summary>
    /// Extension methods for <see cref="ICommandRouter{T}"/>. 
    /// Implementing on the interface as per <see href="https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/extension-methods">MSDN</see>.
    /// </summary>
    public static class ICommandRouterExtensions
    {
        public static async Task SendResponseMessage(this ICommandRouter<CloudEvent> commandRouter, CloudEvent originalCommand, string outgoingData)
        { 
            ArgumentNullException.ThrowIfNull(commandRouter);
            ArgumentNullException.ThrowIfNull(originalCommand);
            ArgumentNullException.ThrowIfNull(outgoingData);

            var responseMessage = new CloudEvent(originalCommand.Source, originalCommand.Type, outgoingData);
            responseMessage.SetCorrelationId(originalCommand.GetCorrelationId());

            await commandRouter.PubSubClient!.Publish(commandRouter.OutgoingTopic!, responseMessage);
        }
    }
}
