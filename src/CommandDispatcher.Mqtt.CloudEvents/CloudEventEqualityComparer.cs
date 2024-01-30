using CloudNative.CloudEvents;
using System.Diagnostics.CodeAnalysis;

namespace CommandDispatcher.Mqtt.CloudEvents
{
    public class CloudEventEqualityComparer : IEqualityComparer<CloudEvent>
    {
        public bool Equals(CloudEvent? ce1, CloudEvent? ce2)
        {
            if (ce1 is null && ce2 is null)
            {
                return true;
            }
            else if (ce1 is null || ce2 is null)
            {
                return false;
            }
            else if (
                Equals(ce1.Id, ce2.Id) &&
                Equals(ce1.Type, ce2.Type) &&
                Equals(ce1.Source, ce2.Source) &&
                Equals(ce1.Subject, ce2.Subject) &&
                Equals(ce1.Data?.ToString(), ce2.Data?.ToString()) &&
                Equals(ce1.Time, ce2.Time) &&
                Equals(ce1.DataSchema, ce2.DataSchema) &&
                Equals(ce1.GetCorrelationId(), ce2.GetCorrelationId()))
                return true;
            else 
                return false;
        }

        public int GetHashCode([DisallowNull] CloudEvent obj)
        {
            return obj.GetHashCode();
        }
    }
}
