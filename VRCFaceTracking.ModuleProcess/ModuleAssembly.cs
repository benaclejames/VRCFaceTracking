using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.ModuleProcess;
public class ModuleAssembly
{
    public Assembly Assembly;
    public string ModulePath;
    public bool Loaded;
    private ILogger<ModuleProcessMain> _logger;
    private ILoggerFactory? _loggerFactory;
    public ExtTrackingModule TrackingModule;

    public ModuleAssembly(ILogger<ModuleProcessMain> logger, ILoggerFactory loggerFactory, string dllPath)
    {
        if ( !File.Exists(dllPath) )
        {
            throw new ArgumentException($"Invalid file path: \"{dllPath}\" does not exist!");
        }
        if ( Path.GetExtension(dllPath.ToLowerInvariant()) != ".dll" && Path.GetExtension(dllPath.ToLowerInvariant()) != "dll" )
        {
            throw new ArgumentException($"{dllPath} is not a DLL file and cannot be loaded.");
        }

        _logger         = logger;
        _loggerFactory  = loggerFactory;
        ModulePath      = dllPath;
        Loaded          = false;
    }

    public void TryLoadAssembly()
    {
        if ( Loaded )
        {
            return;
        }

        try
        {
            var alc = new AssemblyLoadContext(ModulePath, true);
            Assembly = alc.LoadFromAssemblyPath(ModulePath);

            var references = Assembly.GetReferencedAssemblies();
            var oldRefs = false;
            foreach ( var reference in references )
            {
                if ( reference.Name == "VRCFaceTracking" || reference.Name == "VRCFaceTracking.Core" )
                {
                    if ( reference.Version < new Version(5, 0, 0, 0) )
                    {
                        _logger.LogWarning("Module {dll} references an older version of VRCFaceTracking. Skipping.", Path.GetFileName(ModulePath));
                        oldRefs = true;
                    }
                }
            }
            if ( oldRefs )
            {
                return;
            }

            foreach ( var type in Assembly.GetExportedTypes() )
            {
                if ( type.BaseType != typeof(ExtTrackingModule) )
                {
                    continue;
                }

                _logger.LogDebug("{module} properly implements ExtTrackingModule.", type.Name);
                Loaded          = true;
                TrackingModule  = LoadExternalModule();
                break;
            }
        } catch ( Exception e )
        {
            _logger.LogWarning("{error} Assembly not able to be loaded. Skipping.", e.Message);
        }
    }

    private ExtTrackingModule LoadExternalModule()
    {
        if ( Assembly == null )
        {
            throw new Exception("Assembly failed to load but tried setting up module!");
        }

        _logger.LogInformation("Loading External Module " + Assembly.FullName);

        try
        {
            // Get the first class that implements ExtTrackingModule
            var module = Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(ExtTrackingModule)));
            if ( module == null )
            {
                throw new Exception("Failed to get module's ExtTrackingModule impl");
            }
            var moduleObj = (ExtTrackingModule)Activator.CreateInstance(module);
            var logger = _loggerFactory.CreateLogger(moduleObj.GetType().Name);
            moduleObj.Logger = logger;

            return moduleObj;
        } catch ( Exception e )
        {
            _logger.LogError("Exception loading {dll}. Skipping. {e}", Assembly.FullName, e);
        }

        return null;
    }
}
