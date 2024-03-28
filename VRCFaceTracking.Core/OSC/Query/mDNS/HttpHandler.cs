using System.Net;

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
        string respStr;
        if (context.Request.RawUrl.Contains("HOST_INFO"))
        {
            var hostInfo = new OscQueryHostInfo();
            hostInfo.name = "VRCFaceTracking";
            hostInfo.oscIP = "127.0.0.1";
            hostInfo.oscPort = 6969;
            respStr = hostInfo.ToString();
        }
        else
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

            respStr = responseNode.ToString();
        }

        // Send Response
        context.Response.Headers.Add("pragma:no-cache");

        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = respStr.Length;
        using (var sw = new StreamWriter(context.Response.OutputStream))
        {
            await sw.WriteAsync(respStr);
            await sw.FlushAsync();
        }

    }

    public void Dispose()
    {
        _listener.Stop();
    }
}