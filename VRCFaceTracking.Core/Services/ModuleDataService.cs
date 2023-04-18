using System.Net;
using System.Net.Mime;
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
    
    private List<RemoteTrackingModule> _allModules;
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
        if (_allModules == null)
        {
            _allModules = new List<RemoteTrackingModule>(AllModules());
        }

        await Task.CompletedTask;
        return _allModules;
    }

    public async Task<int> GetMyRatingAsync(RemoteTrackingModule module)
    {
        var client = new HttpClient();
        var rating = new RatingObject{UserId = _identityService.GetUniqueUserId(), ModuleId = module.ModuleId.ToString()};
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
        return ratingResponse.Rating;
    }

    public Task SetMyRatingAsync(RemoteTrackingModule module, int rating)
    {
        // Same format as get but we PUT this time
        var client = new HttpClient();
        var ratingObject = new RatingObject{UserId = _identityService.GetUniqueUserId(), ModuleId = module.ModuleId.ToString(), Rating = rating};
        var content = new StringContent(JsonConvert.SerializeObject(ratingObject), Encoding.UTF8, "application/json");
        var response = client.PutAsync("https://rjlk4u22t36tvqz3bvbkwv675a0wbous.lambda-url.us-east-1.on.aws/rating", content).Result;
        return Task.CompletedTask;
    }
}