// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Pathology;

[Serializable, NetSerializable]
public enum DiseaseDiagnoserUiKey : byte
{
    Key
}

/// <summary>
/// Sent by the client to ask the diagnoser to scan the inserted sample.
/// </summary>
[Serializable, NetSerializable]
public sealed class DiseaseDiagnoserScanMessage : BoundUserInterfaceMessage;

/// <summary>
/// Sent by the client to pull stable mutagen out of the inserted container into the machine buffer.
/// </summary>
[Serializable, NetSerializable]
public sealed class DiseaseDiagnoserTransferMutagenMessage : BoundUserInterfaceMessage;

/// <summary>
/// Sent by the client to transcribe the sample's virus into a fresh bottle, spending buffer mutagen.
/// </summary>
[Serializable, NetSerializable]
public sealed class DiseaseDiagnoserCopyMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class DiseaseDiagnoserBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly bool HasSample;
    public readonly bool Scanning;

    /// <summary>Scan progress, 0..1.</summary>
    public readonly float Progress;

    public readonly bool HasResult;

    /// <summary>Virus name, set only when every symptom of a single virus is readable.</summary>
    public readonly string? VirusName;

    /// <summary>Descriptions of symptoms diagnoser could read.</summary>
    public readonly List<string> Symptoms;

    /// <summary>How many symptoms could not be decoded (the "unreadable" hint).</summary>
    public readonly int UnreadableCount;

    /// <summary>Union of the fully-decoded viruses spread vectors, for the "ways it spreads" line.</summary>
    public readonly VirusTransmissionVector Transmission;

    /// <summary>Stable mutagen currently held in the machine buffer.</summary>
    public readonly float BufferMutagen;

    public DiseaseDiagnoserBoundUserInterfaceState(
        bool hasSample,
        bool scanning,
        float progress,
        bool hasResult,
        string? virusName,
        List<string> symptoms,
        int unreadableCount,
        VirusTransmissionVector transmission,
        float bufferMutagen)
    {
        HasSample = hasSample;
        Scanning = scanning;
        Progress = progress;
        HasResult = hasResult;
        VirusName = virusName;
        Symptoms = symptoms;
        UnreadableCount = unreadableCount;
        Transmission = transmission;
        BufferMutagen = bufferMutagen;
    }
}
