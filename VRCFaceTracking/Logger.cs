#if DLL
using MelonLoader;
#else
using System;
#endif


namespace VRCFaceTracking
{
    public class Logger
    {
        public static void Msg(string msgStr)
        {
            #if DLL
            MelonLogger.Msg(msgStr);
#else
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + msgStr);
#endif
        }
        
        public static void Warning(string warningStr)
        {
#if DLL
            MelonLogger.Warning(warningStr);
#else
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [WARNING] " + warningStr);
#endif
        }
        
        public static void Error(string errorStr)
        {
#if DLL
            MelonLogger.Error(errorStr);
#else
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " [ERROR] " + errorStr);
#endif
        }
    }
}