// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Pathology;

[Serializable, NetSerializable]
public enum VaccinatorUiKey : byte
{
    Key
}

/// <summary>Client asks the vaccinator to scan the inserted blood sample.</summary>
[Serializable, NetSerializable]
public sealed class VaccinatorScanMessage : BoundUserInterfaceMessage;

/// <summary>Client asks to pull trico out of the inserted container into the buffer.</summary>
[Serializable, NetSerializable]
public sealed class VaccinatorTransferMessage : BoundUserInterfaceMessage;

/// <summary>Client asks to create a vaccine from the cured blood + buffered trico.</summary>
[Serializable, NetSerializable]
public sealed class VaccinatorCreateVaccineMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class VaccinatorBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly bool HasSample;
    public readonly bool Scanning;
    public readonly float Progress;
    public readonly bool HasResult;
    public readonly string? VirusName;
    public readonly List<string> Symptoms;
    public readonly int UnreadableCount;

    /// <summary>Cure reagent names vaccinator read.</summary>
    public readonly List<string> CureReagents;

    /// <summary>True when a cure exists but its key symptom can't be read.</summary>
    public readonly bool CureHidden;

    public readonly float BufferTricordrazine;

    public VaccinatorBoundUserInterfaceState(
        bool hasSample,
        bool scanning,
        float progress,
        bool hasResult,
        string? virusName,
        List<string> symptoms,
        int unreadableCount,
        List<string> cureReagents,
        bool cureHidden,
        float bufferTricordrazine)
    {
        HasSample = hasSample;
        Scanning = scanning;
        Progress = progress;
        HasResult = hasResult;
        VirusName = virusName;
        Symptoms = symptoms;
        UnreadableCount = unreadableCount;
        CureReagents = cureReagents;
        CureHidden = cureHidden;
        BufferTricordrazine = bufferTricordrazine;
    }
}
