using System;


namespace VRCFaceTracking
{
    public static class Logger
    {
        public static void Msg(string msgStr) => Console.WriteLine("["+DateTime.Now.ToString("HH:mm:ss") + "] " + msgStr);

        public static void Warning(string warningStr)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("["+DateTime.Now.ToString("HH:mm:ss") + "] [WARNING] " + warningStr);
            Console.ResetColor();
        }
        
        public static void Error(string errorStr)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("["+DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] " + errorStr);
            Console.ResetColor();
        }
    }
}