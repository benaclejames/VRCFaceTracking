using System.Threading;

namespace VRCFaceTracking
{
    public partial class App
    {
        private readonly Thread _oscThread = new Thread(MainStandalone.Initialize);
        
        public App()
        {
            _oscThread.Start();
        }
    }
}