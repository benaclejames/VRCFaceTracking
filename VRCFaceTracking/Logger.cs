using System;
using System.Collections.ObjectModel;

namespace VRCFaceTracking
{
    public static class Logger
    {
        public static readonly ObservableCollection<Tuple<string, string>> ConsoleOutput = new ObservableCollection<Tuple<string, string>> { new Tuple<string, string>("Logger Initialized...", "White") };
        public static readonly object ConsoleLock = new object();


        public static void Msg(string msgStr)
        {
            var formattedStr = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msgStr;
            
            ConsoleOutput.Add(new Tuple<string, string>(formattedStr, "White"));
        
            Console.WriteLine(formattedStr);
        }

        public static void Warning(string warningStr)
        {
            var formattedStr = "["+DateTime.Now.ToString("HH:mm:ss") + "] [WARNING] " + warningStr;
            
            ConsoleOutput.Add(new Tuple<string, string>(formattedStr, "Yellow"));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(formattedStr);
            Console.ResetColor();
        }
        
        public static void Error(string errorStr)
        {
            var formattedStr = "[" + DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] " + errorStr;
            
            ConsoleOutput.Add(new Tuple<string, string>(formattedStr,"Red"));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(formattedStr);
            Console.ResetColor();
        }
    }
}