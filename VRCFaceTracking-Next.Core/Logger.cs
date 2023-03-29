using System;
using System.Collections.ObjectModel;

namespace VRCFaceTracking
{
    public static class Logger
    {
        public static Action<string> logAction;
        public static readonly object ConsoleLock = new object();


        public static void Msg(string msgStr)
        {
            var timeStamp = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            var formattedStr = timeStamp + msgStr;

            //logAction.Invoke(formattedStr);
        }

        public static void Warning(string warningStr)
        {
            var formattedStr = "["+DateTime.Now.ToString("HH:mm:ss") + "] [WARNING] ";
            

            //logAction.Invoke(formattedStr + warningStr);
        }
        
        public static void Error(string errorStr)
        {
            var formattedStr = "[" + DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] ";

            //logAction.Invoke(formattedStr + errorStr);
        }
    }
}