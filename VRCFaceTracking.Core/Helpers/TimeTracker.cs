using System.Diagnostics;

namespace VRCFaceTracking.Core.Helpers;

public static class TimeTracker
{
    private static readonly Stopwatch _stopwatch;

    static TimeTracker()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public static double ElapsedMilliseconds => _stopwatch.Elapsed.TotalMilliseconds / 1000f;

    public static void Reset()
    {
        _stopwatch.Reset();
        _stopwatch.Start();
    }
}
