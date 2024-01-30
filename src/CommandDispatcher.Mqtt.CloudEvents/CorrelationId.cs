using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;

namespace CommandDispatcher.Mqtt.CloudEvents
{
    /// <summary>
    /// Extension methods for <see cref="CloudEvent"/> to define a CorrelationId.
    /// For guidance on extensions <see href="https://github.com/cloudevents/sdk-csharp/blob/main/docs/guide.md#extension-attributes"/>
    /// </summary>
    public static class CorrelationId
    {
        public const string CorrelationIdHeader = "correlationid";
        public const string CorrelationIdType = "String";

        /// <summary>
        /// <see cref="CloudEventAttribute"/> representing the 'correlationid' extension attribute.
        /// </summary>
        public static CloudEventAttribute CorrelationIdAttribute { get; } =  
            CloudEventAttribute.CreateExtension(CorrelationIdHeader, CloudEventAttributeType.String);

        /// <summary>
        /// <see cref="CloudEventAttribute"/> representing the 'correlationidtype' extension attribute.
        /// </summary>
        public static CloudEventAttribute CorrelationIdTypeAttribute { get; } =
            CloudEventAttribute.CreateExtension("correlationidtype", CloudEventAttributeType.String);

        /// <summary>
        /// A read-only sequence of all attributes related to the sequence extension.
        /// </summary>
        public static IEnumerable<CloudEventAttribute> AllAttributes { get; } =
            new[] { CorrelationIdAttribute, CorrelationIdTypeAttribute }.ToList().AsReadOnly();

        /// <summary>
        /// Sets both the <see cref="CorrelationIdAttribute"/> and <see cref="CorrelationIdTypeAttribute"/> attributes 
        /// for the cloud event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent on which to set the attributes. Must not be null.</param>
        /// <param name="correlationId">The correlation Id value to set. May be null, in which case both attributes 
        /// are removed from <paramref name="cloudEvent"/>.</param>
        /// <returns><paramref name="cloudEvent"/></returns>
        public static CloudEvent SetCorrelationId(this CloudEvent cloudEvent, string? correlationId)
        {
            Validation.CheckNotNull(cloudEvent, nameof(cloudEvent));
            if (correlationId is null)
            {
                cloudEvent[CorrelationIdAttribute] = null;
                cloudEvent[CorrelationIdTypeAttribute] = null;
            } 
            else
            {
                cloudEvent[CorrelationIdAttribute] = correlationId;
                cloudEvent[CorrelationIdTypeAttribute] = CorrelationIdType;
            }
            return cloudEvent;
        }

        /// <summary>
        /// Retrieves the <see cref="CorrelationIdAttribute"/> value from the event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent from which to retrieve the attribute. Must not be null.</param>
        /// <returns>The <see cref="CorrelationIdAttribute"/> value, as a string, or null if the attribute is not set.</returns>
        public static string? GetCorrelationId(this CloudEvent cloudEvent)
        {
            Validation.CheckNotNull(cloudEvent, nameof(cloudEvent));
            return cloudEvent[CorrelationIdAttribute] as string;
        }

        /// <summary>
        /// Retrieves the <see cref="CorrelationIdTypeAttribute"/> value from the event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent from which to retrieve the attribute. Must not be null.</param>
        /// <returns>The <see cref="CorrelationIdTypeAttribute"/> value, as a string, or null if the attribute is not set.</returns>
        public static string? GetCorrelationIdType(this CloudEvent cloudEvent)
        {
            Validation.CheckNotNull(cloudEvent, nameof(cloudEvent));
            return cloudEvent[CorrelationIdTypeAttribute] as string;
        }
    }
}
