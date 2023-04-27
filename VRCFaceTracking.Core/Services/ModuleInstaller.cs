using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Helpers;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleInstaller
{
    private readonly ILogger<ModuleInstaller> _logger;

    public ModuleInstaller(ILogger<ModuleInstaller> logger)
    {
        _logger = logger;
    }

    private async Task DownloadToFile(TrackingModuleMetadata moduleMetadata, string filePath)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(moduleMetadata.DownloadUrl);
        var content = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(filePath, content);
        await Task.CompletedTask;
    }

    private string TryFindModuleDll(string moduleDirectory, TrackingModuleMetadata moduleMetadata)
    {
        // Attempt to find the first DLL. If there's more than one, try find the one with the same name as the module
        var dllFiles = Directory.GetFiles(moduleDirectory, "*.dll");
        
        if (dllFiles.Length == 0)
            return null;

        if (dllFiles.Length == 1)   // If there's only one, just return it
            return Path.GetFileName(dllFiles[0]);
        
        // Else we'll try find the one with the closest name to the module using Levenshtein distance
        var targetFileName = Path.GetFileNameWithoutExtension(moduleMetadata.DownloadUrl);
        var dllFile = dllFiles.Select(x => new { FileName = Path.GetFileNameWithoutExtension(x), Distance = LevenshteinDistance.Calculate(targetFileName, Path.GetFileNameWithoutExtension(x)) }).MinBy(x => x.Distance);

        if (dllFile == null)
        {
            _logger.LogError(
                "Module {module} has no .dll file name specified and no .dll files were found in the extracted zip",
                moduleMetadata.ModuleId);
            return null;
        }
        
        _logger.LogDebug("Module {module} didn't specify a target dll, and contained multiple. Using {dll} as its distance of {distance} was closest to the module name",
            moduleMetadata.ModuleId, dllFile.FileName, dllFile.Distance);
        return Path.GetFileName(dllFile.FileName);
    }
    
    public async Task<string> InstallRemoteModule(TrackingModuleMetadata moduleMetadata)
    {
        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);
        
        // If our download type is not a .dll, we'll download to a temp directory and then extract to the modules directory
        // The module will be contained within a directory corresponding to the module's id which will contain the root of the zip, or the .dll directly
        // as well as a module.json file containing the metadata for the module so we can identify the currently installed version, as well as
        // still support unofficial modules.
        
        // First we need to create the directory for the module. If it already exists, we'll delete it and start fresh.
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, moduleMetadata.ModuleId.ToString());
        UninstallModule(moduleMetadata);
        Directory.CreateDirectory(moduleDirectory);
        
        // Time to download the main files
        var downloadExtension = Path.GetExtension(moduleMetadata.DownloadUrl);
        if (downloadExtension != ".dll")
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), moduleMetadata.ModuleId.ToString());
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);
            var tempZipPath = Path.Combine(tempDirectory, "module.zip");
            await DownloadToFile(moduleMetadata, tempZipPath);
            ZipFile.ExtractToDirectory(tempZipPath, tempDirectory);
            
            // Delete our zip and copy over all files and folders to the new module directory while preserving the directory structure
            File.Delete(tempZipPath);
            foreach (var file in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
            {
                var path = Path.GetDirectoryName(file);
                var newPath = path?.Replace(tempDirectory, moduleDirectory);
                if (newPath != null)
                {
                    Directory.CreateDirectory(newPath);
                    File.Copy(file, Path.Combine(newPath, Path.GetFileName(file)), true);
                }
            }
            Directory.Delete(tempDirectory, true);
            
            // We need to ensure a .dll name is valid in the RemoteTrackingModule model
            moduleMetadata.DllFileName ??= TryFindModuleDll(moduleDirectory, moduleMetadata);
            if (moduleMetadata.DllFileName == null)
            {
                _logger.LogError("Module {module} has no .dll file name specified and no .dll files were found in the extracted zip", moduleMetadata.ModuleId);
                return null;
            }
        }
        else
        {
            moduleMetadata.DllFileName ??= Path.GetFileName(moduleMetadata.DownloadUrl);
            var dllPath = Path.Combine(moduleDirectory, moduleMetadata.DllFileName);
            
            await DownloadToFile(moduleMetadata, dllPath);
            
            _logger.LogDebug("Downloaded module {module} to {dllPath}", moduleMetadata.ModuleId, dllPath);
        }
        
        // Now we can overwrite the module.json file with the latest metadata
        var moduleJsonPath = Path.Combine(moduleDirectory, "module.json");
        await File.WriteAllTextAsync(moduleJsonPath, JsonConvert.SerializeObject(moduleMetadata, Formatting.Indented));
        
        _logger.LogInformation("Installed module {module} to {moduleDirectory}", moduleMetadata.ModuleId, moduleDirectory);
        
        return Path.Combine(moduleDirectory, moduleMetadata.DllFileName);
    }

    public void MarkModuleForDeletion(InstallableTrackingModule module)
    {
        module.InstallationState = InstallState.AwaitingRestart;
        var moduleJsonPath = Path.Combine(Utils.CustomLibsDirectory, module.ModuleId.ToString(), "module.json");
        File.WriteAllText(moduleJsonPath, JsonConvert.SerializeObject(module, Formatting.Indented));
        _logger.LogInformation("Marked module {module} for deletion", module.ModuleId);
    }
    
    public void UninstallModule(TrackingModuleMetadata moduleMetadata)
    {
        _logger.LogDebug("Uninstalling module {module}", moduleMetadata.ModuleId);
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, moduleMetadata.ModuleId.ToString());
        if (Directory.Exists(moduleDirectory))
        {
            try
            {
                Directory.Delete(moduleDirectory, true);
                _logger.LogInformation("Uninstalled module {module} from {moduleDirectory}", moduleMetadata.ModuleId, moduleDirectory);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to uninstall module {module} from {moduleDirectory}", moduleMetadata.ModuleId, moduleDirectory);
            }
        }
        else
        {
            _logger.LogWarning("Module {module} could not be found where it was expected in {moduleDirectory}", moduleMetadata.ModuleId, moduleDirectory);
        }
    }
}