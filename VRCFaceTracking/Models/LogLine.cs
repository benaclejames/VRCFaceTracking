using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Models;

public record LogLine(string Message, LogLevel Level)
{
    public override string ToString() => Message;
}  