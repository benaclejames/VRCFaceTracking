using System;
using System.Threading;
using static VRCFaceTracking.MainWindow;

namespace VRCFaceTracking
{
    public class InputManager
    {
        // Move to the WPF window as a button
        public bool ShouldPause;

        public InputManager() => new Thread(Listen).Start();

        private void Listen()
        {
            return;
            while (true)
            {
                var key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.P)
                {
                    ShouldPause = !ShouldPause;
                    Logger.Msg(ShouldPause ? "Paused" : "Unpaused");
                }
            }
        }
    }
}