using System;

namespace AutomaticRoadblocks
{
    public static class Assert
    {
        public static void NotNull(object value, string message)
        {
            if (value == null)
                throw new ArgumentException(message, (Exception)null);
        }

        public static void HasText(string value, string message)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(message);
        }

        public static void True(bool statement, string message)
        {
            if (!statement)
                throw new ArgumentException(message);
        }
    }
}