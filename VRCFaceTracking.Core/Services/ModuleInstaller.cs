using System.IO.Compression;
using System.Net;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleInstaller
{
    public async Task<string> InstallRemoteModule(RemoteTrackingModule module)
    {
        // If our download type is not a .dll, we'll download to a temp directory and then extract to the modules directory
        // The module will be contained within a directory corresponding to the module's id which will contain the root of the zip, or the .dll directly
        // as well as a module.json file containing the metadata for the module so we can identify the currently installed version, as well as
        // still support unofficial modules.
        var moduleDirectory = Path.Combine(Utils.CustomLibsDirectory, module.ModuleId.ToString());
        if (Directory.Exists(moduleDirectory))
        {
            Directory.Delete(moduleDirectory, true);
        }
        Directory.CreateDirectory(moduleDirectory);
        
        // Time to download the main files
        var downloadExtension = Path.GetExtension(module.DownloadUrl);
        if (downloadExtension != ".dll")
        {
            // We need to ensure a .dll name is valid in the RemoteTrackingModule model
            if (string.IsNullOrWhiteSpace(module.DllFileName))
            {
                throw new InvalidDataException("The module's DllName property must be set if the download url does not end in .dll");
            }
            
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
            var extractedDirectory = Directory.GetDirectories(tempDirectory).First();
            var extractedFiles = Directory.GetFiles(extractedDirectory);
            foreach (var extractedFile in extractedFiles)
            {
                var fileName = Path.GetFileName(extractedFile);
                var destinationPath = Path.Combine(moduleDirectory, fileName);
                File.Copy(extractedFile, destinationPath);
            }
            Directory.Delete(tempDirectory, true);
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
        }
        
        // Now we can overwrite the module.json file with the latest metadata
        var moduleJsonPath = Path.Combine(moduleDirectory, "module.json");
        await File.WriteAllTextAsync(moduleJsonPath, JsonConvert.SerializeObject(module, Formatting.Indented));
        
        return Path.Combine(moduleDirectory, module.DllFileName);
    }
}