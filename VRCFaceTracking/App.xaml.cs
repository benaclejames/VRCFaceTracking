using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace VRCFaceTracking
{
    public partial class App
    {
        private readonly Thread _oscThread = new Thread(MainStandalone.Initialize);
        
        public App()
        {
            BindingOperations.EnableCollectionSynchronization(Logger.ConsoleOutput, Logger.ConsoleLock);
            _oscThread.Start();
        }

        ~App() => MainStandalone.Teardown();

        private void App_OnExit(object sender, ExitEventArgs e) => MainStandalone.Teardown();
    }
}