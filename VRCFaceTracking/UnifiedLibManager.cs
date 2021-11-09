using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        // Used when re-initializing modules, kills malfunctioning SRanipal process and restarts it. 
        public static IEnumerator CheckRuntimeSanity()
        {
            EyeEnabled = false;
            LipEnabled = false;
            UsefulModules.Clear();
            foreach (var process in Process.GetProcessesByName("sr_runtime"))
            {
                MelonLogger.Msg("Killing "+process.ProcessName);
                process.Kill();
                yield return new WaitForSeconds(3);
                MelonLogger.Msg("Re-Initializing");
                Initialize();
            }
        }
        
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
                bool eyeSuccess = false, lipSuccess = false;    // Init result for every 
                
                var moduleObj = (ITrackingModule) Activator.CreateInstance(module);
                // If there is still a need for a module with eye or lip tracking and this module supports the current need, try initialize it
                if (!EyeEnabled && moduleObj.SupportsEye || !LipEnabled && moduleObj.SupportsLip)
                    (eyeSuccess, lipSuccess) = moduleObj.Initialize(eye, lip);

                // If the module successfully initialized anything, add it to the list of useful modules and start its update thread
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

        // Signal all active modules to gracefully shut down their respective runtimes
        public static void Teardown()
        {
            foreach (var module in UsefulModules)
                module.Value.Teardown();
        }
        
        // Manually signal all useful modules to get the latest data
        public static void Update()
        {
            if (ShouldThread || !(EyeEnabled || LipEnabled)) return;
            
            foreach (var module in UsefulModules.Values)
                module.Update();
        }
    }
}