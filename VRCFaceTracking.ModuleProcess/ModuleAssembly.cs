using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.ModuleProcess;
public class ModuleAssembly
{
    private Assembly? Assembly
    {
        get
        {
            if (field == null)
            {
                field = TryLoadAssembly();
            }

            return field;
        }
    }

    public ExtTrackingModule? TrackingModule
    {
        get
        {
            if (field == null)
            {
                field = LoadExternalModule();
            }

            return field;
        }
    }
    
    public string ModulePath;
    private ILogger<ModuleProcessMain> _logger;
    private ILoggerFactory? _loggerFactory;
    public CancellationTokenSource? _updateCts;

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
    }

    private Assembly? TryLoadAssembly()
    {
        try
        {
            var alc = new AssemblyLoadContext(ModulePath, true);
            return alc.LoadFromAssemblyPath(ModulePath);
        }
        catch (Exception e)
        {
            _logger.LogWarning("{error} Assembly not able to be loaded. Skipping.", e.Message);
        }

        return null;
    }

    private ExtTrackingModule? LoadExternalModule()
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
