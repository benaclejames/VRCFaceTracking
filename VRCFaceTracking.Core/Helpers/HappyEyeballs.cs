using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Helpers;

// Credit: https://slugcat.systems/post/24-06-16-ipv6-is-hard-happy-eyeballs-dotnet-httpclient/
public static class HappyEyeballsHttp
{
    private const int ConnectionAttemptDelay = 250;

#if DEBUG
    private const int SlowIpv6 = 0;
    private const bool BrokenIpv6 = false;
#endif

    // .NET does not implement Happy Eyeballs at the time of writing.
    // https://github.com/space-wizards/SS14.Launcher/issues/38
    // This is the workaround.
    //
    // What's Happy Eyeballs? It makes the launcher try both IPv6 and IPv4,
    // the former with priority, so that if IPv6 is broken your launcher still works.
    //
    // Implementation originally based on,
    // rewritten as to be nigh-impossible to recognize https://github.com/ppy/osu-framework/pull/4191/files
    //
    // This is a simple implementation. It does not fully implement RFC 8305:
    // * We do not separately handle parallel A and AAAA DNS requests as optimization.
    // * We don't sort IPs as specified in RFC 6724. I can't tell if GetHostEntryAsync does.
    // * Look I wanted to keep this simple OK?
    //   We don't do any fancy shit like statefulness or incremental sorting
    //   or incremental DNS updates who cares about that.
    public static HttpClient CreateHttpClient(bool autoRedirect = true)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = OnConnect,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = autoRedirect,
            // PooledConnectionLifetime = TimeSpan.FromSeconds(1)
        };

        return new HttpClient(handler);
    }

    private static async ValueTask<Stream> OnConnect(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        // Get IPs via DNS.
        // Note that we do not attempt to exclude IPv6 if the user doesn't have IPv6.
        // According to the docs, GetHostEntryAsync will not return them if there's no address.
        // BUT! I tested and that's a lie at least on Linux.
        // Regardless, if you don't have IPv6,
        // an attempt to connect to an IPv6 socket *should* immediately give a "network unreachable" socket error.
        // This will cause the code to immediately try the next address,
        // so IPv6 just gets "skipped over" if you don't have it.
        // I could find no other robust way to check "is there a chance in hell IPv6 works" other than "try it",
        // so... try it we will.
        var endPoint = context.DnsEndPoint;
        var resolvedAddresses = await GetIpsForHost(endPoint, cancellationToken).ConfigureAwait(false);
        if (resolvedAddresses.Length == 0)
            throw new Exception($"Host {context.DnsEndPoint.Host} resolved to no IPs!");

        // Sort as specified in the RFC, interleaving.
        var ips = SortInterleaved(resolvedAddresses);

        Debug.Assert(ips.Length > 0);

        var (socket, index) = await ParallelTask(
            ips.Length,
            (i, cancel) => AttemptConnection(i, ips[i], endPoint.Port, cancel),
            TimeSpan.FromMilliseconds(ConnectionAttemptDelay),
            cancellationToken);

        Debug.WriteLine($"Successfully connected {endPoint} to address: {ips[index]}");

        return new NetworkStream(socket, ownsSocket: true);
    }

    private static async Task<Socket> AttemptConnection(
        int index,
        IPAddress address,
        int port,
        CancellationToken cancel)
    {
        Debug.WriteLine($"Trying IP {address} for happy eyeballs [{index}]");

        // The following socket constructor will create a dual-mode socket on systems where IPV6 is available.
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
            NoDelay = true
        };

        try
        {
#if DEBUG
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                await Task.Delay(SlowIpv6, cancel).ConfigureAwait(false);

                if (BrokenIpv6)
                    throw new Exception("Oh no I can't reach the network this is SO SAD.");
            }
#endif

            await socket.ConnectAsync(new IPEndPoint(address, port), cancel).ConfigureAwait(false);
            return socket;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Happy Eyeballs to {address} [{index}] failed");
            socket.Dispose();
            throw;
        }
    }

    private static async Task<IPAddress[]> GetIpsForHost(DnsEndPoint endPoint, CancellationToken cancel)
    {
        if (IPAddress.TryParse(endPoint.Host, out var ip))
            return [ip];

        var entry = await Dns.GetHostEntryAsync(endPoint.Host, cancel).ConfigureAwait(false);
        return entry.AddressList;
    }

    private static IPAddress[] SortInterleaved(IPAddress[] addresses)
    {
        // Interleave returned addresses so that they are IPv6 -> IPv4 -> IPv6 -> IPv4.
        // Assuming we have multiple addresses of the same type that is.
        // As described in the RFC.

        var ipv6 = addresses.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6).ToArray();
        var ipv4 = addresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();

        var commonLength = Math.Min(ipv6.Length, ipv4.Length);

        var result = new IPAddress[addresses.Length];
        for (var i = 0; i < commonLength; i++)
        {
            result[i * 2] = ipv6[i];
            result[1 + i * 2] = ipv4[i];
        }

        if (ipv4.Length > ipv6.Length)
        {
            ipv4.AsSpan(commonLength).CopyTo(result.AsSpan(commonLength * 2));
        }
        else if (ipv6.Length > ipv4.Length)
        {
            ipv6.AsSpan(commonLength).CopyTo(result.AsSpan(commonLength * 2));
        }

        return result;
    }

    internal static async Task<(T, int)> ParallelTask<T>(
        int candidateCount,
        Func<int, CancellationToken, Task<T>> taskBuilder,
        TimeSpan delay,
        CancellationToken cancel) where T : IDisposable
    {
        if (candidateCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(candidateCount), "Value must be positive and non-zero.");

        using var successCts = CancellationTokenSource.CreateLinkedTokenSource(cancel);

        // All tasks we have ever tried.
        var allTasks = new List<Task<T>>();
        // Tasks we are still waiting on.
        var tasks = new List<Task<T>>();

        // The general loop here is as follows:
        // 1. Add a new task for the next IP to try.
        // 2. Wait until any task completes OR the delay happens.
        // If an error occurs, we stop checking that task and continue checking the next.
        // Every iteration we add another task, until we're full on them.
        // We keep looping until we have SUCCESS, or we run out of attempt tasks entirely.

        Task<T>? successTask = null;
        while (successTask == null && (allTasks.Count < candidateCount || tasks.Count > 0))
        {
            if (allTasks.Count < candidateCount)
            {
                // We have to queue another task this iteration.
                var newTask = taskBuilder(allTasks.Count, successCts.Token);
                tasks.Add(newTask);
                allTasks.Add(newTask);
            }

            var whenAnyDone = Task.WhenAny(tasks);
            Task<T> completedTask;

            if (allTasks.Count < candidateCount)
            {
                Debug.WriteLine("Waiting on ConnectionAttemptDelay");
                // If we have another one to queue, wait for a timeout instead of *just* waiting for a connection task.
                var timeoutTask = Task.Delay(delay, successCts.Token);
                var whenAnyOrTimeout = await Task.WhenAny(whenAnyDone, timeoutTask).ConfigureAwait(false);
                if (whenAnyOrTimeout != whenAnyDone)
                {
                    // Timeout finished. Go to next iteration so we queue another one.
                    continue;
                }

                completedTask = whenAnyDone.Result;
            }
            else
            {
                completedTask = await whenAnyDone.ConfigureAwait(false);
            }

            if (completedTask.IsCompletedSuccessfully)
            {
                // We did it. We have success.
                successTask = completedTask;
                break;
            }
            else
            {
                // Faulted. Remove it.
                tasks.Remove(completedTask);
            }
        }

        Debug.Assert(allTasks.Count > 0);

        cancel.ThrowIfCancellationRequested();
        successCts.Cancel();

        if (successTask == null)
        {
            // We didn't get a single successful connection. Well heck.
            throw new AggregateException(
                allTasks.Where(x => x.IsFaulted).SelectMany(x => x.Exception!.InnerExceptions));
        }

        // I don't know if this is possible but MAKE SURE that we don't get two sockets completing at once.
        // Just a safety measure.
        foreach (var task in allTasks)
        {
            if (task.IsCompletedSuccessfully && task != successTask)
                task.Result.Dispose();
        }

        return (successTask.Result, allTasks.IndexOf(successTask));
    }
}
