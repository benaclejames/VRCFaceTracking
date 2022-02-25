using System;
using System.Threading;

namespace VRCFaceTracking
{
    public class InputManager
    {
        public bool ShouldPause;
        
        public InputManager() => new Thread(Listen).Start();

        private void Listen()
        {
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