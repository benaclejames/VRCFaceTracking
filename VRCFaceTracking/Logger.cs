using System;
using System.Collections.ObjectModel;
using MelonLoader;

namespace VRCFaceTracking
{
    public static class Logger
    { 
        public static void Msg(string msgStr)
        {
            MelonLogger.Msg(msgStr);
        }

        public static void Warning(string warningStr)
        {
            MelonLogger.Warning(warningStr);
        }
        
        public static void Error(string errorStr)
        {
            MelonLogger.Error(errorStr);
        }
    }
}