// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

namespace Content.Server.SS220.Pathology;

public static class PathologyMachine
{
    public static float ComputeScanProgress(TimeSpan? scanEndTime, TimeSpan scanDuration, bool hasResult, TimeSpan now)
    {
        if (scanEndTime is not { } end)
            return hasResult ? 1f : 0f;

        var remaining = (float)(end - now).TotalSeconds;
        var duration = (float)scanDuration.TotalSeconds;
        return duration <= 0f ? 1f : Math.Clamp(1f - remaining / duration, 0f, 1f);
    }
}
