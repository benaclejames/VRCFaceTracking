using System.Net;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class HttpHandler : IDisposable
{
    private readonly HttpListener _listener = new();
    private IAsyncResult _contextListenerResult;
    private readonly IOscTarget _oscTarget;
    private readonly ILogger<HttpHandler> _logger;
    private string _appName = "VRCFT";
    
    public HttpHandler(IOscTarget oscTarget, ILogger<HttpHandler> logger)
    {
        _oscTarget = oscTarget;
        _logger = logger;
    }
    
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

    public void SetAppName(string newAppName)
    {
        _appName = newAppName;
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
                name = _appName,
                oscIP = _oscTarget.DestinationAddress,
                oscPort = _oscTarget.InPort
            };
            respStr = hostInfo.ToString();
            _logger.LogDebug($"Responding to oscquery host info request with {respStr}");
        }
        else
        {
            if (context.Request.Url.LocalPath != "/")
                return; // Not properly implementing oscquery protocol because I'm unemployed and not being paid to

            var responseNode = new OSCQueryNode("/")
            {
                Description = "root node",
                Access = AccessValues.NoValue
            };

            var avatarNode = new OSCQueryNode("/avatar")
            {
                Access = AccessValues.NoValue
            };

            var avatarChangeNode = new OSCQueryNode("/avatar/change")
            {
                Access = AccessValues.WriteOnly,
                OscType = "s"
            };

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