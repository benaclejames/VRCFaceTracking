using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleDataService : IModuleDataService
{
    private List<RemoteTrackingModule> _remoteModules;
    private readonly Dictionary<Guid, int> _ratingCache = new();

    private readonly IIdentityService _identityService;
    private readonly ILogger<ModuleDataService> _logger;

    public ModuleDataService(IIdentityService identityService, ILogger<ModuleDataService> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    private static IEnumerable<RemoteTrackingModule> AllModules()
    {
        // This is where we make the actual request to the API at https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/modules
        // and get the list of modules.
        var client = new HttpClient();
        var response = client.GetAsync("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/modules").Result;
        var content = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<List<RemoteTrackingModule>>(content);
    }

    public async Task<IEnumerable<RemoteTrackingModule>> GetListDetailsDataAsync()
    {
        _remoteModules ??= new List<RemoteTrackingModule>(AllModules());

        await Task.CompletedTask;
        return _remoteModules;
    }
    
    public IEnumerable<LocalTrackingModule> GetInstalledModules() => AllInstalled();

    public Task IncrementDownloadsAsync(RemoteTrackingModule module)
    {
        // send a PATCH request to https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/downloads with the module ID in the body
        var client = new HttpClient();
        var rating = new RatingObject
            { UserId = _identityService.GetUniqueUserId(), ModuleId = module.ModuleId.ToString() };
        var content = new StringContent(JsonConvert.SerializeObject(rating), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Patch,
            RequestUri = new Uri("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/downloads"),
            Content = content,
        };
        var response = client.SendAsync(request).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Failed to increment downloads for {ModuleId}. Status code: {StatusCode}", module.ModuleId, response.StatusCode);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetLegacyModules()
    {
        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);

        return Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll");
    }

    public async Task<int> GetMyRatingAsync(RemoteTrackingModule module)
    {
        if (_ratingCache.TryGetValue(module.ModuleId, out var async))
        {
            _logger.LogDebug("Rating for {ModuleId} was cached as {Rating}", module.ModuleId, async);
            return async;
        }

        var client = new HttpClient();
        var rating = new RatingObject
            { UserId = _identityService.GetUniqueUserId(), ModuleId = module.ModuleId.ToString() };
        var content = new StringContent(JsonConvert.SerializeObject(rating), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/rating"),
            Content = content,
        };
        var response = await client.SendAsync(request);
        // Deserialize the input content but extract the Rating property. Be careful though, we might 404 if the user hasn't rated the module yet.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Rating for {ModuleId} was not found", module.ModuleId);
            _ratingCache[module.ModuleId] = 0;
            return 0;
        }

        var ratingResponse = JsonConvert.DeserializeObject<RatingObject>(response.Content.ReadAsStringAsync().Result);
        
        _logger.LogDebug("Rating for {ModuleId} was {Rating}. Caching...", module.ModuleId, ratingResponse.Rating);
        _ratingCache[module.ModuleId] = ratingResponse.Rating;
        return ratingResponse.Rating;
    }

    public Task SetMyRatingAsync(RemoteTrackingModule module, int rating)
    {
        // Same format as get but we PUT this time
        var client = new HttpClient();
        var ratingObject = new RatingObject{UserId = _identityService.GetUniqueUserId(), ModuleId = module.ModuleId.ToString(), Rating = rating};
        var content = new StringContent(JsonConvert.SerializeObject(ratingObject), Encoding.UTF8, "application/json");
        var response = client.PutAsync("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/rating", content).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Failed to set rating for {ModuleId} to {Rating}. Status code: {StatusCode}", module.ModuleId, rating, response.StatusCode);
            return Task.CompletedTask;
        }
        _logger.LogDebug("Rating for {ModuleId} was set to {Rating}. Caching...", module.ModuleId, rating);
        _ratingCache[module.ModuleId] = rating;
        return Task.CompletedTask;
    }

    private IEnumerable<LocalTrackingModule> AllInstalled()
    {
        // Check each folder in our CustomModulesDir folder and see if it has a module.json file.
        // If it does, deserialize it and add it to the list of installed modules.
        var installedModules = new List<LocalTrackingModule>();
        var moduleFolders = Directory.GetDirectories(Utils.CustomLibsDirectory);
        foreach (var moduleFolder in moduleFolders)
        {
            var moduleJsonPath = Path.Combine(moduleFolder, "module.json");
            if (File.Exists(moduleJsonPath))
            {
                var moduleJson = File.ReadAllText(moduleJsonPath);
                try
                {
                    var module = JsonConvert.DeserializeObject<LocalTrackingModule>(moduleJson);
                    module.InstallationState = InstallState.Installed;
                    module.AssemblyLoadPath = Path.Combine(moduleFolder, module.DllFileName);
                    installedModules.Add(module);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to deserialize module.json for {ModuleFolder}", moduleFolder);
                }
            }
            else
            {
                
            }
        }
        
        // Now we account for legacy modules
        var moduleDlls = Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll");
        foreach (var moduleDll in moduleDlls)
        {
            var module = new LocalTrackingModule
            {
                AssemblyLoadPath = moduleDll,
                DllFileName = Path.GetFileName(moduleDll),
                InstallationState = InstallState.Installed,
                ModuleId = Guid.Empty,
                ModuleName = Path.GetFileNameWithoutExtension(moduleDll),
                ModuleDescription = "Legacy module",
                AuthorName = "Local",
                ModulePageUrl = "file:///"+Path.GetDirectoryName(moduleDll)
            };
            installedModules.Add(module);
        }
        
        return installedModules;
    }
}