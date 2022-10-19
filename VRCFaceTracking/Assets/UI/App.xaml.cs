using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace VRCFaceTracking.Assets.UI
{
    public partial class App
    {
        private readonly MainStandalone Standalone;
        private readonly Thread ProcessingThread;
        
        public App()
        {
            BindingOperations.EnableCollectionSynchronization(Logger.ConsoleOutput, Logger.ConsoleLock);
            Standalone = new MainStandalone();
            ProcessingThread = new Thread(Standalone.Initialize);
            ProcessingThread.Start();
        }

        ~App()
        {
            ProcessingThread.Abort();
            Standalone.Dispose();
        }

        private void App_OnExit(object sender, ExitEventArgs e) => Standalone.Dispose();
    }
}