using System.Net;
using VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class HttpHandler : IDisposable
{
    private readonly HttpListener _listener;
    
    public HttpHandler(int port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        _listener.Start();
        _listener.BeginGetContext(HttpListenerLoop, _listener);
    }
    
    async void HttpListenerLoop(IAsyncResult result)
    {
        var context = _listener.EndGetContext(result);
        _listener.BeginGetContext(HttpListenerLoop, _listener);
        if (!context.Request.RawUrl.Contains("HOST_INFO"))
        {
            if (context.Request.Url.LocalPath != "/")
                return; // Not properly implementing oscquery protocol because I'm unemployed and not being paid to

            var responseNode = new OSCQueryNode("/");
            responseNode.Description = "root node";
            responseNode.Access = AccessValues.NoValue;

            var avatarNode = new OSCQueryNode("/avatar");
            avatarNode.Access = AccessValues.NoValue;

            var avatarChangeNode = new OSCQueryNode("/avatar/change");
            avatarChangeNode.Access = AccessValues.WriteOnly;
            avatarChangeNode.OscType = "s";

            avatarNode.Contents = new Dictionary<string, OSCQueryNode>()
            {
                { "change", avatarChangeNode }
            };

            responseNode.Contents = new Dictionary<string, OSCQueryNode>()
            {
                { "avatar", avatarNode }
            };

            var stringResponse = responseNode.ToString();
            context.Response.Headers.Add("pragma:no-cache");

            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = stringResponse.Length;
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                await sw.WriteAsync(stringResponse);
                await sw.FlushAsync();
            }

            return;
        }

        try
        {
            // Serve Host Info for requests with "HOST_INFO" in them
            var hostInfo = new OscQueryHostInfo();
            hostInfo.name = "VRCFaceTracking";
            hostInfo.oscIP = "127.0.0.1";
            hostInfo.oscPort = 6969;
            var hostInfoString = hostInfo.ToString();

            // Send Response
            context.Response.Headers.Add("pragma:no-cache");

            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = hostInfoString.Length;
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                await sw.WriteAsync(hostInfoString);
                await sw.FlushAsync();
            }
        }
        catch (Exception e)
        {
        }
    }
    
    public void Dispose()
    {
        _listener.Stop();
    }
}