using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.LipMerging;
using VRCFaceTracking.QuickMenu;

namespace VRCFaceTracking
{
    public interface ITrackingModule
    {
        bool SupportsEye { get; }
        bool SupportsLip { get; }

        (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip);
        void StartThread();
        void Update();
        void Teardown();
    }

    public static class UnifiedLibManager
    {
        public static bool EyeEnabled, LipEnabled;
        private static readonly Dictionary<Type, ITrackingModule> UsefulModules = new Dictionary<Type, ITrackingModule>();
        
        private static Thread _initializeWorker;
        public static readonly bool ShouldThread = !Environment.GetCommandLineArgs().Contains("--vrcft-nothread");

        public static void Initialize(bool eye = true, bool lip = true)
        {
            if (_initializeWorker != null && _initializeWorker.IsAlive) _initializeWorker.Abort();
            _initializeWorker = new Thread(() => FindAndInitRuntimes(eye, lip));
            _initializeWorker.Start();
        }

        private static void FindAndInitRuntimes(bool eye = true, bool lip = true)
        {
            IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
            
            var trackingModules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ITrackingModule).IsAssignableFrom(type) && !type.IsInterface);

            foreach (var module in trackingModules)
            {
                bool eyeSuccess = false, lipSuccess = false;
                var moduleObj = (ITrackingModule) Activator.CreateInstance(module);
                if (!EyeEnabled && moduleObj.SupportsEye || !LipEnabled && moduleObj.SupportsLip)
                    (eyeSuccess, lipSuccess) = moduleObj.Initialize(eye, lip);

                if ((eyeSuccess || lipSuccess) && !UsefulModules.ContainsKey(module))
                {
                    UsefulModules.Add(module, moduleObj);
                    if (ShouldThread)
                        moduleObj.StartThread();
                }

                if (eyeSuccess) EyeEnabled = true;
                if (lipSuccess) LipEnabled = true;

                if (EyeEnabled && LipEnabled) break;    // Keep enumerating over all modules until we find ones we can use
            }

            if (eye)
                MelonLogger.Msg(EyeEnabled
                    ? "Eye Tracking Initialized"
                    : "Eye Tracking will be unavailable for this session.");

            if (lip)
            {
                if (LipEnabled) MelonLogger.Msg("Lip Tracking Initialized");
                else MelonLogger.Warning("Lip Tracking will be unavailable for this session.");
            }

            if (SceneManager.GetActiveScene().buildIndex == -1 && QuickModeMenu.MainMenu != null)
                MainMod.MainThreadExecutionQueue.Add(() => QuickModeMenu.MainMenu.UpdateEnabledTabs(EyeEnabled, LipEnabled));
        }

        public static void Teardown()
        {
            foreach (var module in UsefulModules)
                module.Value.Teardown();
        }
        
        public static void Update()
        {
            if (ShouldThread) return;
            
            foreach (var module in UsefulModules.Values)
                module.Update();
        }
    }
}