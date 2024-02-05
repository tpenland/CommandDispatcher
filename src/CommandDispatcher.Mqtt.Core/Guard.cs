using System.Runtime.CompilerServices;

namespace CommandDispatcher.Mqtt.Core
{
    public static class Guard
    {
        public static void IsNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string name = "")
        {
            if (value is null)
            {
                throw new ArgumentException(name);
            }
        }

        public static void IsEmpty<T>(IList<T>? list, [CallerArgumentExpression(nameof(list))] string name = "")
        {
            if (list is null || list.Count == 0)
            {
                throw new ArgumentException(name);
            }
        }
    }
}
