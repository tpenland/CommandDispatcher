namespace CommandDispatcher.Mqtt.Core
{
    public static class Guard
    {
        public static void IsNotNull(object value, string name)
        {
            if (value is null || value.Equals(default))
            {
                throw new ArgumentException(name);
            }
        }

        public static void IsNotEmpty<T>(IList<T> list, string name)
        {
            if (list is null || list.Count == 0)
            {
                throw new ArgumentException(name);
            }
        }
    }
}
