using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace VRCFaceTracking.Assets.UI
{
    public partial class App
    {
        private readonly Thread _oscThread = new Thread(MainStandalone.Initialize);
        
        public App()
        {
            try
            {
                BindingOperations.EnableCollectionSynchronization(Logger.ConsoleOutput, Logger.ConsoleLock);
                _oscThread.Start();
            }
            catch ( Exception e ) 
            {
                string filePath = Utils.PersistentDataDirectory + "/ExceptionLog.txt";

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Date : " + DateTime.Now.ToString() + "\n");

                    while (e != null)
                    {
                        writer.WriteLine
                        (
                            e.GetType().FullName + "\n" + 
                            "Message : " + e.Message + "\n" + 
                            "StackTrace : " + e.StackTrace
                        );

                        e = e.InnerException;
                    }

                    writer.Close();
                }

                Logger.Error(e.Message);
                Logger.Msg("Please restart VRCFaceTracking to reinitialize the application.");
            }
        }

        ~App() => MainStandalone.Teardown();

        private void App_OnExit(object sender, ExitEventArgs e) => MainStandalone.Teardown();
    }
}