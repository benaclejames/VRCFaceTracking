using System.Net;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class HttpHandler : IDisposable
{
    private readonly HttpListener _listener = new();
    private IAsyncResult _contextListenerResult;

    public void BindTo(string uri)
    {
        if (_contextListenerResult != null)
        {
            _listener.EndGetContext(_contextListenerResult);
        }
        _listener.Stop();
        _listener.Prefixes.Clear();
        _listener.Prefixes.Add(uri);
        _listener.Start();
        _contextListenerResult = _listener.BeginGetContext(HttpListenerLoop, _listener);
    }

    private async void HttpListenerLoop(IAsyncResult result)
    {
        var context = _listener.EndGetContext(result);
        _listener.BeginGetContext(HttpListenerLoop, _listener);
        string respStr;
        if (context.Request.RawUrl.Contains("HOST_INFO"))
        {
            var hostInfo = new OscQueryHostInfo
            {
                name = "VRCFaceTracking",
                oscIP = "127.0.0.1",
                oscPort = 6969
            };
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