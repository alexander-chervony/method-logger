using Configuration;

namespace MethodLogger
{
    public class WeavedNotifier
    {
        private static readonly IErrorLogger _errorLogger;
        private static readonly InvocationLogger _invocationLogger;

        static WeavedNotifier()
        {
            _errorLogger = new ErrorLogger();
            var config = new Config(_errorLogger);
            _invocationLogger = new InvocationLogger(_errorLogger, config, new MethodLoggerRepository(config));
        }

        public static void NotifyEnter(string methodName, int methodIndex)
        {
            _invocationLogger.Log(methodName, methodIndex);
        }

        public static void NotifyExit(string methodName)
        {
        }

        public static void NotifyJumpOut(string methodName)
        {
        }

        public static void NotifyJumpBack(string methodName)
        {
        }
    }
}