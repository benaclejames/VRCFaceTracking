using MelonLoader;

namespace VRCFaceTracking
{
    public static class Logger
    {
        public static MelonLogger.Instance logger;

        public static void Msg(string msgStr) => logger.Msg(msgStr);

        public static void Warning(string warningStr) => logger.Warning(warningStr);

        public static void Error(string errorStr) => logger.Error(errorStr);
    }
}