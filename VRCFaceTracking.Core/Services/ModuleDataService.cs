using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleDataService : IModuleDataService
{
    private List<InstallableTrackingModule> _remoteModules;
    private readonly Dictionary<Guid, int> _ratingCache = new();

    private readonly IIdentityService _identityService;
    private readonly ILogger<ModuleDataService> _logger;

    public ModuleDataService(IIdentityService identityService, ILogger<ModuleDataService> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    private static IEnumerable<InstallableTrackingModule> AllModules()
    {
        // This is where we make the actual request to the API at https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/modules
        // and get the list of modules.
        var client = new HttpClient();
        var response = client.GetAsync("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/modules").Result;
        var content = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<List<InstallableTrackingModule>>(content);
    }

    public async Task<IEnumerable<InstallableTrackingModule>> GetRemoteModules()
    {
        _remoteModules ??= new List<InstallableTrackingModule>(AllModules());

        await Task.CompletedTask;
        return _remoteModules;
    }

    public Task IncrementDownloadsAsync(TrackingModuleMetadata moduleMetadata)
    {
        // send a PATCH request to https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/downloads with the module ID in the body
        var client = new HttpClient();
        var rating = new RatingObject
            { UserId = _identityService.GetUniqueUserId(), ModuleId = moduleMetadata.ModuleId.ToString() };
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
            _logger.LogError("Failed to increment downloads for {ModuleId}. Status code: {StatusCode}", moduleMetadata.ModuleId, response.StatusCode);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    public IEnumerable<InstallableTrackingModule> GetLegacyModules()
    {
        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);
        
        var legacyModules = new List<InstallableTrackingModule>();
        var moduleDlls = Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll");
        foreach (var moduleDll in moduleDlls)
        {
            var module = new InstallableTrackingModule
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
            legacyModules.Add(module);
        }

        return legacyModules;
    }

    public async Task<int> GetMyRatingAsync(TrackingModuleMetadata moduleMetadata)
    {
        if (_ratingCache.TryGetValue(moduleMetadata.ModuleId, out var async))
        {
            _logger.LogDebug("Rating for {ModuleId} was cached as {Rating}", moduleMetadata.ModuleId, async);
            return async;
        }

        var client = new HttpClient();
        var rating = new RatingObject
            { UserId = _identityService.GetUniqueUserId(), ModuleId = moduleMetadata.ModuleId.ToString() };
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
            _logger.LogDebug("Rating for {ModuleId} was not found", moduleMetadata.ModuleId);
            _ratingCache[moduleMetadata.ModuleId] = 0;
            return 0;
        }

        var ratingResponse = JsonConvert.DeserializeObject<RatingObject>(response.Content.ReadAsStringAsync().Result);
        
        _logger.LogDebug("Rating for {ModuleId} was {Rating}. Caching...", moduleMetadata.ModuleId, ratingResponse.Rating);
        _ratingCache[moduleMetadata.ModuleId] = ratingResponse.Rating;
        return ratingResponse.Rating;
    }

    public Task SetMyRatingAsync(TrackingModuleMetadata moduleMetadata, int rating)
    {
        // Same format as get but we PUT this time
        var client = new HttpClient();
        var ratingObject = new RatingObject{UserId = _identityService.GetUniqueUserId(), ModuleId = moduleMetadata.ModuleId.ToString(), Rating = rating};
        var content = new StringContent(JsonConvert.SerializeObject(ratingObject), Encoding.UTF8, "application/json");
        var response = client.PutAsync("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/rating", content).Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Failed to set rating for {ModuleId} to {Rating}. Status code: {StatusCode}", moduleMetadata.ModuleId, rating, response.StatusCode);
            return Task.CompletedTask;
        }
        _logger.LogDebug("Rating for {ModuleId} was set to {Rating}. Caching...", moduleMetadata.ModuleId, rating);
        _ratingCache[moduleMetadata.ModuleId] = rating;
        return Task.CompletedTask;
    }

    public IEnumerable<InstallableTrackingModule> GetInstalledModules()
    {
        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);
        
        // Check each folder in our CustomModulesDir folder and see if it has a module.json file.
        // If it does, deserialize it and add it to the list of installed modules.
        var installedModules = new List<InstallableTrackingModule>();
        var moduleFolders = Directory.GetDirectories(Utils.CustomLibsDirectory);
        foreach (var moduleFolder in moduleFolders)
        {
            var moduleJsonPath = Path.Combine(moduleFolder, "module.json");
            if (!File.Exists(moduleJsonPath))
                continue;

            var moduleJson = File.ReadAllText(moduleJsonPath);
            try
            {
                var module = JsonConvert.DeserializeObject<InstallableTrackingModule>(moduleJson);
                module.AssemblyLoadPath = Path.Combine(moduleFolder, module.DllFileName);
                installedModules.Add(module);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deserialize module.json for {ModuleFolder}", moduleFolder);
            }
        }

        return installedModules;
    } }