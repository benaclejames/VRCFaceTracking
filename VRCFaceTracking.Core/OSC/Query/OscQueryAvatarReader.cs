using Newtonsoft.Json;

namespace VRCFaceTracking.Core.OSC.Query;

internal static class OscQueryAvatarReader
{
    internal static async Task<OscQueryNode> ReadAsync(HttpClient http, Uri avatarEndpoint)
    {
        try
        {
            return await ReadNodeAsync(http, avatarEndpoint);
        }
        catch (JsonException)
        {
            // VRC did an update that causes the response length to be wrong 
            // when the avi name has unicode characters
            // So now we read parts separately
            // Epic meows
        }

        var avatarBase = new Uri(avatarEndpoint.AbsoluteUri.TrimEnd('/') + "/");
        var changeEndpoint = new Uri(avatarBase, "change");
        var nativeTrackingEndpoint = new Uri(avatarBase, "avatarfullfacetracking");
        var parametersEndpoint = new Uri(avatarBase, "parameters");

        for(var att = 0; att < 2; att++){
            var prev = await ReadNodeAsync(http, changeEndpoint);
            var prevId = ReadAvatarId(prev);
            var parameters = await ReadNodeAsync(http, parametersEndpoint);
            ValidateAvatarParameters(parameters);

            //OscQueryAvatarInfo flag
            var nativeTracking = await ReadNativeTrackingAsync(http, nativeTrackingEndpoint);
            var afterId = ReadAvatarId(await ReadNodeAsync(http, changeEndpoint));
            if(prevId != afterId) continue;

            return new OscQueryNode("/avatar")
            {
                Contents = new Dictionary<string, OscQueryNode>
                {
                    ["change"] = prev,
                    ["parameters"] = parameters,
                    ["avatarfullfacetracking"] = nativeTracking,
                }
            };
        }

        throw new JsonSerializationException("Avatar changed while reading its OSCQuery data");
    }

    private static async Task<OscQueryNode> ReadNodeAsync(HttpClient http, Uri uri)
    {
        return JsonConvert.DeserializeObject<OscQueryNode>(await http.GetStringAsync(uri))
            ?? throw new JsonSerializationException("OSCQuery returned a null node");
    }

    private static string ReadAvatarId(OscQueryNode change)
    {
        if(change.FullPath != "/avatar/change" || change.Value is not { Length: 1 } 
            || change.Value[0] is not string id 
            || string.IsNullOrWhiteSpace(id))
            throw new JsonSerializationException("OSCQuery returned an invalid avatar identity");
        return id;
    }

    private static void ValidateAvatarParameters(OscQueryNode parameters)
    {
        if(parameters.FullPath != "/avatar/parameters" 
            || parameters.Contents == null)
            throw new JsonSerializationException("OSCQuery returned an invalid avatar parameter tree");
    }

    private static async Task<OscQueryNode> ReadNativeTrackingAsync(HttpClient http, Uri uri)
    {
        OscQueryNode oscNode;

        try
        {
            oscNode = await ReadNodeAsync(http, uri);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new OscQueryNode("/avatar/avatarfullfacetracking") { Value = [false] };
        }

        if(oscNode.FullPath != "/avatar/avatarfullfacetracking"
            || oscNode.Value is not { Length: 1 }
            || oscNode.Value[0] is not bool)
            throw new JsonSerializationException("OSCQuery returned an invalid native tracking node");
        return oscNode;
    }
}