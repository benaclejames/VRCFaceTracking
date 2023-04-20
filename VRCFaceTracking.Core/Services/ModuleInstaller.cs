using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleInstaller
{
    private readonly ILogger<ModuleInstaller> _logger;

    public ModuleInstaller(ILogger<ModuleInstaller> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> InstallRemoteModule(RemoteTrackingModule module)
    {
        // If our download type is not a .dll, we'll download to a temp directory and then extract to the modules directory
        // The module will be contained within a directory corresponding to the module's id which will contain the root of the zip, or the .dll directly
        // as well as a module.json file containing the metadata for the module so we can identify the currently installed version, as well as
        // still support unofficial modules.
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, module.ModuleId.ToString());
        if (Directory.Exists(moduleDirectory))
        {
            _logger.LogDebug("Deleting existing module directory {moduleDirectory}", moduleDirectory);
            try
            {
                Directory.Delete(moduleDirectory, true);
            }
            catch (Exception)
            {
                _logger.LogWarning("Failed to delete existing module directory {moduleDirectory}", moduleDirectory);
                return "";
            }
        }
        Directory.CreateDirectory(moduleDirectory);
        
        // Time to download the main files
        var downloadExtension = Path.GetExtension(module.DownloadUrl);
        if (downloadExtension != ".dll")
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), module.ModuleId.ToString());
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);
            var tempZipPath = Path.Combine(tempDirectory, "module.zip");
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(module.DownloadUrl);
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(tempZipPath, content);
            }
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
            if (string.IsNullOrWhiteSpace(module.DllFileName))
            {
                // Attempt to find the first DLL. If there's more than one, try find the one with the same name as the module
                var dllFiles = Directory.GetFiles(moduleDirectory, "*.dll");
                if (dllFiles.Length == 0)
                {
                    _logger.LogError("Module {module} has no .dll file name specified and no .dll files were found in the extracted zip", module.ModuleId);
                    return null;
                }
                if (dllFiles.Length == 1)
                {
                    _logger.LogWarning("Module {module} has no .dll file name specified and only one .dll file was found in the extracted zip. We'll assume {dllFile} is the correct file", module.ModuleId, Path.GetFileName(dllFiles[0]));
                    module.DllFileName = Path.GetFileName(dllFiles[0]);
                }
                else
                {
                    var dllFile = dllFiles.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == module.ModuleName);
                    if (dllFile == null)
                    {
                        _logger.LogError("Module {module} has no .dll file name specified and multiple .dll files were found in the extracted zip", module.ModuleId);
                        return null;
                    }
                    module.DllFileName = Path.GetFileName(dllFile);
                }
            }
        }
        else
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(module.DownloadUrl);
            var content = await response.Content.ReadAsByteArrayAsync();
            if (string.IsNullOrWhiteSpace(module.DllFileName))
            {
                module.DllFileName = Path.GetFileName(module.DownloadUrl);
            }
            var dllPath = Path.Combine(moduleDirectory, module.DllFileName);
            await File.WriteAllBytesAsync(dllPath, content);
            _logger.LogDebug("Downloaded module {module} to {dllPath}", module.ModuleId, dllPath);
        }
        
        // Now we can overwrite the module.json file with the latest metadata
        var moduleJsonPath = Path.Combine(moduleDirectory, "module.json");
        await File.WriteAllTextAsync(moduleJsonPath, JsonConvert.SerializeObject(module, Formatting.Indented));
        
        _logger.LogInformation("Installed module {module} to {moduleDirectory}", module.ModuleId, moduleDirectory);
        
        return Path.Combine(moduleDirectory, module.DllFileName);
    }
    
    public void UninstallModule(RemoteTrackingModule module)
    {
        _logger.LogDebug("Uninstalling module {module}", module.ModuleId);
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, module.ModuleId.ToString());
        if (Directory.Exists(moduleDirectory))
        {
            try
            {
                Directory.Delete(moduleDirectory, true);
                _logger.LogInformation("Uninstalled module {module} from {moduleDirectory}", module.ModuleId, moduleDirectory);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to uninstall module {module} from {moduleDirectory}", module.ModuleId, moduleDirectory);
            }
        }
        else
        {
            _logger.LogWarning("Module {module} could not be found where it was expected in {moduleDirectory}", module.ModuleId, moduleDirectory);
        }
    }
}