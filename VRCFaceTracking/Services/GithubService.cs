using System.Net.Http.Headers;
using System.Text.Json;
using VRCFaceTracking.Models;

namespace VRCFaceTracking.Services;

public class GithubService
{
    public async Task<List<GithubContributor>> GetContributors(string repo)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VRCFaceTracking", "1.0"));
        var response = await client.GetAsync($"https://api.github.com/repos/{repo}/contributors");
        if (!response.IsSuccessStatusCode)
        {
            return new List<GithubContributor>();
        }
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<GithubContributor>>(content);
    }
}