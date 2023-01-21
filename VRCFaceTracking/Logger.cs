using System;
using System.Collections.ObjectModel;
using System.IO;

namespace VRCFaceTracking
{
    public static class Logger
    {
        public static readonly ObservableCollection<Tuple<string, string>> ConsoleOutput = new ObservableCollection<Tuple<string, string>>();
        public static readonly object ConsoleLock = new object();
        private static readonly string LogFilePath;

        static Logger()
        {
            LogFilePath = Path.Combine(Utils.PersistentDataDirectory, "VRCFT_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
        }

        private static string WalkStackAndGetModuleName()
        {
            // First, we need to grab the callstack
            var stack = new System.Diagnostics.StackTrace();

            // Now walk the stack and look for any declaring types that are derivative of ExtTrackingModule
            for (var i = 0; i < stack.FrameCount; i++)
            {
                var frame = stack.GetFrame(i);
                var type = frame.GetMethod().DeclaringType;
                if (type != null && type.IsSubclassOf(typeof(ExtTrackingModule)))
                {
                    // We found a type that is a subclass of ExtTrackingModule, so return the name of the type
                    return "["+type.Name+"] ";
                }
            }
            
            // If we didn't find anything, walk the stack and find the calling type, and return that
            for (var i = 0; i < stack.FrameCount; i++)
            {
                var frame = stack.GetFrame(i);
                var type = frame.GetMethod().DeclaringType;
                if (type != null && type != typeof(Logger))
                {
                    return "["+type.Name+"] ";
                }
            }

            return "";
        }
        
        private static string GetFormattedDateTime() => "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";


        public static void Msg(string msgStr)
        {
            var formattedStr = GetFormattedDateTime() + WalkStackAndGetModuleName() + msgStr;
            
            File.AppendAllText(LogFilePath, formattedStr+"\n");
            
            lock (ConsoleLock)
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr, "White"));
        }

        public static void Warning(string warningStr)
        {
            var formattedStr = GetFormattedDateTime() + WalkStackAndGetModuleName() + "[WARNING] " + warningStr;

            File.AppendAllText(LogFilePath, formattedStr+"\n");
            
            lock (ConsoleLock)
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr, "Yellow"));
        }
        
        public static void Error(string errorStr)
        {
            var formattedStr = GetFormattedDateTime() + WalkStackAndGetModuleName() +"[ERROR] " + errorStr;

            File.AppendAllText(LogFilePath, formattedStr+"\n");
            
            lock (ConsoleLock)
                ConsoleOutput.Add(new Tuple<string, string>(formattedStr,"Red"));
        }
    }
}