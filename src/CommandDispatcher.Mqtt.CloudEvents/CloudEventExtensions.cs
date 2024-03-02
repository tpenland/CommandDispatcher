using Azure.Messaging;

namespace CommandDispatcher.Mqtt.CloudEvents
{
    /// <summary>
    /// Extension methods for <see cref="CloudEvent"/>.
    /// </summary>
    public static class CloudEventExtensions
    {
        public const string CorrelationIdName = "correlationid";
        public const string CorrelationIdType = "string";

        /// <summary>
        /// This method is used to create a CloudEvent with a correlationId.
        /// </summary>
        /// <param name=""></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public static CloudEvent CloudEventFactory(this CloudEvent cloudEvent, string source, string type, string data, string correlationId)
        {
            var ce = new CloudEvent(source, type, data);
            ce.SetCorrelationId(correlationId);
            return ce;
        }

        /// <summary>
        /// Sets both the CorrelationId and CorrelationIdType for the cloud event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent on which to set the attributes. Must not be null.</param>
        /// <param name="correlationId">The correlation Id value to set. May be null, in which case both attributes 
        /// are removed from <paramref name="cloudEvent"/>.</param>
        /// <returns><paramref name="cloudEvent"/></returns>
        public static CloudEvent SetCorrelationId(this CloudEvent cloudEvent, string? correlationId)
        {
            ArgumentNullException.ThrowIfNull(cloudEvent);

            if (correlationId is null)
            {
                cloudEvent.ExtensionAttributes.Remove(CorrelationIdName);
                cloudEvent.ExtensionAttributes.Remove(CorrelationIdType);
            }
            else
            {
                cloudEvent.ExtensionAttributes[CorrelationIdName] = correlationId;
                cloudEvent.ExtensionAttributes[CorrelationIdType] = CorrelationIdType;
            }
            return cloudEvent;
        }

        /// <summary>
        /// Retrieves the CorrelationId value from the event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent from which to retrieve the attribute. Must not be null.</param>
        /// <returns>TheCorrelationId value, as a string, or null if the attribute is not set.</returns>
        public static string? GetCorrelationId(this CloudEvent cloudEvent)
        {
            ArgumentNullException.ThrowIfNull(cloudEvent);
            cloudEvent.ExtensionAttributes.TryGetValue(CorrelationIdName, out var correlationId);
            return correlationId?.ToString();
        }

        /// <summary>
        /// Retrieves the CorrelationIdType value from the event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent from which to retrieve the attribute. Must not be null.</param>
        /// <returns>The CorrelationIdType value, as a string, or null if the attribute is not set.</returns>
        public static string? GetCorrelationIdType(this CloudEvent cloudEvent)
        {
            ArgumentNullException.ThrowIfNull(cloudEvent);
            cloudEvent.ExtensionAttributes.TryGetValue(CorrelationIdType, out var correlationId);
            return correlationId?.ToString();
        }

    }
}
