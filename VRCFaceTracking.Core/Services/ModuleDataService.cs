using System.Net;
using System.Text;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleDataService : IModuleDataService
{
    private struct RatingObject
    {
        public string UserId { get; set; }
        public string ModuleId { get; set; }
        public int Rating { get; set; }
    }
    
    private List<RemoteTrackingModule> _remoteModules, _installedModules;
    private Dictionary<Guid, int> _ratingCache = new Dictionary<Guid, int>();

    private IIdentityService _identityService;
    
    public ModuleDataService(IIdentityService identityService)
    {
        _identityService = identityService;
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
        if (_remoteModules == null)
        {
            _remoteModules = new List<RemoteTrackingModule>(AllModules());
        }

        await Task.CompletedTask;
        return _remoteModules;
    }
    
    public async Task<IEnumerable<RemoteTrackingModule>> GetInstalledModulesAsync()
    {
        if (_installedModules == null)
        {
            _installedModules = new List<RemoteTrackingModule>(AllInstalled());
        }

        await Task.CompletedTask;
        return _installedModules;
    }

    public async Task<int> GetMyRatingAsync(RemoteTrackingModule module)
    {
        if (_ratingCache.TryGetValue(module.ModuleId, out var async))
            return async;
        
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
            return 0;
        var ratingResponse = JsonConvert.DeserializeObject<RatingObject>(response.Content.ReadAsStringAsync().Result);
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
        _ratingCache[module.ModuleId] = rating;
        return Task.CompletedTask;
    }

    public static IEnumerable<RemoteTrackingModule> AllInstalled()
    {
        // Check each folder in our CustomModulesDir folder and see if it has a module.json file.
        // If it does, deserialize it and add it to the list of installed modules.
        var installedModules = new List<RemoteTrackingModule>();
        var moduleFolders = Directory.GetDirectories(Utils.CustomLibsDirectory);
        foreach (var moduleFolder in moduleFolders)
        {
            var moduleJsonPath = Path.Combine(moduleFolder, "module.json");
            if (File.Exists(moduleJsonPath))
            {
                var moduleJson = File.ReadAllText(moduleJsonPath);
                var module = JsonConvert.DeserializeObject<RemoteTrackingModule>(moduleJson);
                installedModules.Add(module);
            }
        }
        
        return installedModules;
    }
}