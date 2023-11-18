using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Helpers;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleInstaller
{
    private readonly ILogger _logger;

    public ModuleInstaller(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("ModuleInstaller");
        
        if (!Directory.Exists(Utils.CustomLibsDirectory))
        {
            Directory.CreateDirectory(Utils.CustomLibsDirectory);
        }
    }

    // Move a directory using just Copy and Remove as MoveDirectory is not usable across drives
    private void MoveDirectory(string source, string dest)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(dest))
        {
            return;
        }
        
        if (!Directory.Exists(dest))
        {
            Directory.CreateDirectory(dest);
        }

        // Get files recursively and preserve directory structure
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var path = Path.GetDirectoryName(file);
            var newPath = path?.Replace(source, dest);
            if (newPath == null)
            {
                continue;
            }

            Directory.CreateDirectory(newPath);
            File.Copy(file, Path.Combine(newPath, Path.GetFileName(file)), true);
        }
        
        // Now we delete the source directory
        Directory.Delete(source, true);
    }

    private async Task DownloadModuleToFile(TrackingModuleMetadata moduleMetadata, string filePath)
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
        
        switch (dllFiles.Length)
        {
            case 0:
                return null;
            // If there's only one, just return it
            case 1:
                return Path.GetFileName(dllFiles[0]);
        }

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

    public async Task<string> InstallLocalModule(string zipPath)
    {
        // First, we copy the zip to our custom libs directory
        var fileName = Path.GetFileName(zipPath);
        var newZipPath = Path.Combine(Utils.CustomLibsDirectory, fileName);
        File.Copy(zipPath, newZipPath, true);
        
        // Second, we unzip it 
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(zipPath));
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }
        Directory.CreateDirectory(tempDirectory);
        ZipFile.ExtractToDirectory(newZipPath, tempDirectory);
        File.Delete(newZipPath);

        // Now, we need to find the module.json file and deserialize it
        var moduleJsonPath = Path.Combine(tempDirectory, "module.json");
        if (!File.Exists(moduleJsonPath))
        {
            _logger.LogError("Module {module} does not contain a module.json file", fileName);
            Directory.Delete(tempDirectory, true);
            return null;
        }
        
        var moduleMetadata = await Json.ToObjectAsync<TrackingModuleMetadata>(await File.ReadAllTextAsync(moduleJsonPath));
        if (moduleMetadata == null)
        {
            _logger.LogError("Module {module} contains an invalid module.json file", fileName);
            Directory.Delete(tempDirectory, true);
            return null;
        }
        
        // Now we move to a directory named after the module id and delete the temp directory
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, moduleMetadata.ModuleId.ToString());
        if (Directory.Exists(moduleDirectory))
        {
            Directory.Delete(moduleDirectory, true);
        }

        MoveDirectory(tempDirectory, moduleDirectory);
        
        // Now we need to find the module's dll
        moduleMetadata.DllFileName ??= TryFindModuleDll(moduleDirectory, moduleMetadata);
        if (moduleMetadata.DllFileName == null)
        {
            _logger.LogError("Module {module} has no .dll file name specified and no .dll files were found in the extracted zip", moduleMetadata.ModuleId);
            return null;
        }
        
        // Now we write the module.json file to the module directory
        await File.WriteAllTextAsync(Path.Combine(moduleDirectory, "module.json"), JsonConvert.SerializeObject(moduleMetadata, Formatting.Indented));
        
        // Finally, we return the module's dll file name
        return Path.Combine(moduleDirectory, moduleMetadata.DllFileName);
    }

    public async Task<string> InstallRemoteModule(TrackingModuleMetadata moduleMetadata)
    {
        // If our download type is not a .dll, we'll download to a temp directory and then extract to the modules directory
        // The module will be contained within a directory corresponding to the module's id which will contain the root of the zip, or the .dll directly
        // as well as a module.json file containing the metadata for the module so we can identify the currently installed version, as well as
        // still support unofficial modules.
        
        // First we need to create the directory for the module. If it already exists, we'll delete it and start fresh.
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, moduleMetadata.ModuleId.ToString());
        UninstallModule(moduleMetadata);

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
            await DownloadModuleToFile(moduleMetadata, tempZipPath);
            ZipFile.ExtractToDirectory(tempZipPath, tempDirectory);
            
            // Delete our zip and copy over all files and folders to the new module directory while preserving the directory structure
            File.Delete(tempZipPath);
            MoveDirectory(tempDirectory, moduleDirectory);
            
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
            
            if (!Directory.Exists(moduleDirectory))
            {
                Directory.CreateDirectory(moduleDirectory);
            }

            await DownloadModuleToFile(moduleMetadata, dllPath);
            
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