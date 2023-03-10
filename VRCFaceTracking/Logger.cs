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
            var timeStamp = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";
            var formattedStr = timeStamp + msgStr;

            try
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr, "White"));
                Console.WriteLine(formattedStr);
            }
            catch (Exception e)
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr + "Message intercepted while writing. " + e, "White"));
                Console.WriteLine(formattedStr);
            }
        }

        public static void Warning(string warningStr)
        {
            var formattedStr = "["+DateTime.Now.ToString("HH:mm:ss") + "] [WARNING] ";
            Console.ForegroundColor = ConsoleColor.Yellow;

            try
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr + warningStr, "Yellow"));
                Console.WriteLine(formattedStr);
            }
            catch (Exception e)
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr + "Message intercepted while writing. " + e, "Yellow"));
                Console.WriteLine(formattedStr);
            }

            Console.ResetColor();
        }
        
        public static void Error(string errorStr)
        {
            var formattedStr = "[" + DateTime.Now.ToString("HH:mm:ss") + "] [ERROR] ";
            Console.ForegroundColor = ConsoleColor.Red;

            try
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr + errorStr, "Red"));
                Console.WriteLine(formattedStr);
                
            }
            catch (Exception e)
            {
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr + "Message intercepted while writing. " + e, "Red"));
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.ResetColor();
        }
    }
}