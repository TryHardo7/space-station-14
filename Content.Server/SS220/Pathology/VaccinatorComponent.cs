// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.SS220.Pathology;

[RegisterComponent]
public sealed partial class VaccinatorComponent : Component
{
    [DataField]
    public string SlotId = "vaccinatorSlot";

    [DataField]
    public TimeSpan ScanDuration = TimeSpan.FromSeconds(4);

    [DataField]
    public string BufferSolutionId = "buffer";

    /// <summary>Reagent consumed from the buffer to create vaccine.</summary>
    [DataField]
    public ProtoId<ReagentPrototype> TricordrazineReagent = "Tricordrazine";

    /// <summary>Reagent the produced vaccine is made of (carries the immunity data).</summary>
    [DataField]
    public ProtoId<ReagentPrototype> VaccineReagent = "Vaccine";

    /// <summary>How much tricordrazine one vaccine costs, and how much vaccine ends up in the bottle.</summary>
    [DataField]
    public FixedPoint2 VaccineAmount = 15;

    [DataField]
    public EntProtoId VaccineBottle = "ChemistryEmptyVial";

    [DataField]
    public string BottleSolutionId = "drink";

    public TimeSpan? ScanEndTime;
    public TimeSpan NextUiUpdate;

    // --- last scan result ---

    public bool HasResult;
    public string? ResultVirusName;
    public List<string> ResultSymptoms = new();
    public int ResultUnreadableCount;
    public List<string> ResultCureReagents = new();
    public bool ResultCureHidden;
}
