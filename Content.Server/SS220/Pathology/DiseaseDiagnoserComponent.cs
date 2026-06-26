// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.SS220.Pathology;
using Robust.Shared.Prototypes;

namespace Content.Server.SS220.Pathology;

[RegisterComponent]
public sealed partial class DiseaseDiagnoserComponent : Component
{
    /// <summary>
    /// Item slot holding the sample container.
    /// </summary>
    [DataField]
    public string SlotId = "diagnoserSlot";

    /// <summary>
    /// How long a scan takes.
    /// </summary>
    [DataField]
    public TimeSpan ScanDuration = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Internal buffer solution that holds stable mutagen for copying.
    /// </summary>
    [DataField]
    public string BufferSolutionId = "buffer";

    /// <summary>
    /// Mutagen in the buffer at or above this amount lights up the filled-buffer sprite overlay.
    /// </summary>
    [DataField]
    public FixedPoint2 BufferDisplayThreshold = 50;

    /// <summary>
    /// Reagent used to copy a virus.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> MutagenReagent = "StableMutagen";

    /// <summary>
    /// Unstable mutagen reagent — also lights the mutagen vial sprite, but isn't usable for copying.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> UnstableMutagenReagent = "UnstableMutagen";

    /// <summary>
    /// Amount of reagent one copy costs, also amount ends up in the produced bottle.
    /// </summary>
    [DataField]
    public FixedPoint2 CopyAmount = 15;

    /// <summary>
    /// Container spawned to hold a copied virus.
    /// </summary>
    [DataField]
    public EntProtoId CopyBottle = "ChemistryEmptyVial";

    /// <summary>
    /// Solution in produced bottle.
    /// </summary>
    [DataField]
    public string BottleSolutionId = "drink";

    /// <summary>
    /// When the current scan finishes.
    /// </summary>
    public TimeSpan? ScanEndTime;

    /// <summary>
    /// Throttle for pushing progress updates to UI while scanning.
    /// </summary>
    public TimeSpan NextUiUpdate;

    // last scan results below

    public bool HasResult;
    public string? ResultVirusName;
    public List<string> ResultSymptoms = new();
    public int ResultUnreadableCount;

    /// <summary>
    /// Final spread vectors dedcoded virus shows.
    /// </summary>
    public VirusTransmissionVector ResultTransmission;
}
