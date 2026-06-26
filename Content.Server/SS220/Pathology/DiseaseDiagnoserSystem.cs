// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using System.Linq;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Server.Power.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.SS220.Pathology;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.SS220.Pathology;

public sealed partial class DiseaseDiagnoserSystem : EntitySystem
{
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedPathologySystem _pathology = default!;

    private static readonly TimeSpan UiUpdateInterval = TimeSpan.FromSeconds(0.2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseDiagnoserComponent, ComponentStartup>(OnUiDirty);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, BoundUIOpenedEvent>(OnUiDirty);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, DiseaseDiagnoserScanMessage>(OnScan);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, DiseaseDiagnoserTransferMutagenMessage>(OnTransferMutagen);
        SubscribeLocalEvent<DiseaseDiagnoserComponent, DiseaseDiagnoserCopyMessage>(OnCopy);
    }

    private void OnUiDirty<T>(Entity<DiseaseDiagnoserComponent> ent, ref T args)
    {
        UpdateUi(ent);
    }

    private void OnEntInserted(Entity<DiseaseDiagnoserComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        OnSlotChanged(ent, args.Container.ID);
    }

    private void OnEntRemoved(Entity<DiseaseDiagnoserComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        OnSlotChanged(ent, args.Container.ID);
    }

    private void OnSlotChanged(Entity<DiseaseDiagnoserComponent> ent, string containerId)
    {
        if (containerId != ent.Comp.SlotId)
            return;

        ent.Comp.ScanEndTime = null;
        ClearResult(ent);
        UpdateUi(ent);
    }

    private void OnScan(Entity<DiseaseDiagnoserComponent> ent, ref DiseaseDiagnoserScanMessage args)
    {
        // the UI stays open after power is cut, so gate the action itself, not just opening
        if (!this.IsPowered(ent, EntityManager))
            return;

        if (ent.Comp.ScanEndTime != null)
            return;

        if (_itemSlots.GetItemOrNull(ent, ent.Comp.SlotId) == null)
            return;

        ClearResult(ent);
        ent.Comp.ScanEndTime = _timing.CurTime + ent.Comp.ScanDuration;
        ent.Comp.NextUiUpdate = _timing.CurTime;
        UpdateUi(ent);
    }

    private void OnTransferMutagen(Entity<DiseaseDiagnoserComponent> ent, ref DiseaseDiagnoserTransferMutagenMessage args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        var item = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId);
        if (item == null
            || !_solutionContainer.TryGetFitsInDispenser(item.Value, out var sourceSoln, out var source)
            || !_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out var bufferSoln, out var buffer))
            return;

        var available = source.GetTotalPrototypeQuantity(ent.Comp.MutagenReagent);
        var amount = FixedPoint2.Min(available, buffer.MaxVolume - buffer.Volume);
        if (amount <= FixedPoint2.Zero)
        {
            _popup.PopupEntity(Loc.GetString("disease-diagnoser-no-mutagen-source"), ent, args.Actor);
            return;
        }

        _solutionContainer.RemoveReagent(sourceSoln.Value, ent.Comp.MutagenReagent, amount);
        _solutionContainer.TryAddReagent(bufferSoln.Value, ent.Comp.MutagenReagent, amount, out _);
        UpdateUi(ent);
    }

    private void OnCopy(Entity<DiseaseDiagnoserComponent> ent, ref DiseaseDiagnoserCopyMessage args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        var item = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId);
        if (item == null || !_solutionContainer.TryGetFitsInDispenser(item.Value, out _, out var source))
            return;

        var viruses = VirusData.EnumerateViruses(source).Select(v => v.Clone()).ToList();
        if (viruses.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("disease-diagnoser-no-virus"), ent, args.Actor);
            return;
        }

        if (!_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out var bufferSoln, out var buffer)
            || buffer.GetTotalPrototypeQuantity(ent.Comp.MutagenReagent) < ent.Comp.CopyAmount)
        {
            _popup.PopupEntity(Loc.GetString("disease-diagnoser-not-enough-mutagen"), ent, args.Actor);
            return;
        }

        var bottle = Spawn(ent.Comp.CopyBottle, Transform(ent).Coordinates);
        if (!_solutionContainer.TryGetSolution(bottle, ent.Comp.BottleSolutionId, out var bottleSoln, out _))
        {
            // misconfigured CopyBottle prototype: bail before consuming mutagen or reporting success
            Log.Error($"CopyBottle {ent.Comp.CopyBottle} has no '{ent.Comp.BottleSolutionId}' solution");
            Del(bottle);
            return;
        }

        _solutionContainer.RemoveReagent(bufferSoln.Value, ent.Comp.MutagenReagent, ent.Comp.CopyAmount);

        var virusData = new VirusData { Viruses = viruses };
        _solutionContainer.TryAddReagent(bottleSoln.Value, ent.Comp.MutagenReagent, ent.Comp.CopyAmount, out _, data: new List<ReagentData> { virusData });

        _popup.PopupEntity(Loc.GetString("disease-diagnoser-copied"), ent, args.Actor);
        UpdateUi(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DiseaseDiagnoserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ScanEndTime is not { } end)
                continue;

            if (_timing.CurTime >= end)
            {
                comp.ScanEndTime = null;
                BuildResult((uid, comp));
                UpdateUi((uid, comp));
                continue;
            }

            if (_timing.CurTime >= comp.NextUiUpdate)
            {
                comp.NextUiUpdate = _timing.CurTime + UiUpdateInterval;
                UpdateUi((uid, comp));
            }
        }
    }

    private void BuildResult(Entity<DiseaseDiagnoserComponent> ent)
    {
        ClearResult(ent);
        ent.Comp.HasResult = true;

        var item = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId);
        if (item == null || !_solutionContainer.TryGetFitsInDispenser(item.Value, out _, out var solution))
            return;

        // a symptom shared by several strains in one sample is reported once, not per strain
        var seen = new HashSet<string>();
        foreach (var virus in VirusData.EnumerateViruses(solution))
        {
            var allReadable = true;
            foreach (var symptomId in virus.Symptoms)
            {
                string? description = null;
                var readable = virus.RevealedSymptoms.Contains(symptomId)
                    && _pathology.TryGetSymptomDescription(symptomId, out description)
                    && description != null;

                if (!readable)
                    allReadable = false;

                if (!seen.Add(symptomId))
                    continue;

                // symptom reported only if revealed, otherwise stay unreadable
                if (readable)
                    ent.Comp.ResultSymptoms.Add(_pathology.FormatSymptom(symptomId, description!, virus, showAccelerant: true));
                else
                    ent.Comp.ResultUnreadableCount++;
            }

            // spread vectors are reported only if every symptom of the strain has been revealed
            if (allReadable)
                ent.Comp.ResultTransmission |= GetVectors(virus.Transmission);

            if (allReadable
                && ent.Comp.ResultVirusName == null
                && virus.Name is { } name)
            {
                ent.Comp.ResultVirusName = name;
            }
        }
    }

    private static VirusTransmissionVector GetVectors(VirusTransmission? transmission)
    {
        var vectors = VirusTransmissionVector.None;
        if (transmission == null)
            return vectors;

        if (transmission.ContactChance > 0f)
            vectors |= VirusTransmissionVector.Contact;

        if (transmission.ProximityChance > 0f)
            vectors |= VirusTransmissionVector.Proximity;

        return vectors;
    }

    private void UpdateUi(Entity<DiseaseDiagnoserComponent> ent)
    {
        var hasSample = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId) != null;
        var scanning = ent.Comp.ScanEndTime != null;
        var progress = PathologyMachine.ComputeScanProgress(ent.Comp.ScanEndTime, ent.Comp.ScanDuration, ent.Comp.HasResult, _timing.CurTime);

        var bufferMutagen = 0f;
        if (_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out _, out var buffer))
            bufferMutagen = (float)buffer.GetTotalPrototypeQuantity(ent.Comp.MutagenReagent);

        var state = new DiseaseDiagnoserBoundUserInterfaceState(
            hasSample,
            scanning,
            progress,
            ent.Comp.HasResult,
            ent.Comp.ResultVirusName,
            new List<string>(ent.Comp.ResultSymptoms),
            ent.Comp.ResultUnreadableCount,
            ent.Comp.ResultTransmission,
            bufferMutagen);

        _ui.SetUiState(ent.Owner, DiseaseDiagnoserUiKey.Key, state);

        UpdateVisuals(ent);
    }

    // Fancy af.
    private void UpdateVisuals(Entity<DiseaseDiagnoserComponent> ent)
    {
        var bufferFull = false;
        if (_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out _, out var buffer))
            bufferFull = buffer.GetTotalPrototypeQuantity(ent.Comp.MutagenReagent) >= ent.Comp.BufferDisplayThreshold;

        _appearance.SetData(ent, DiseaseDiagnoserVisuals.Running, ent.Comp.ScanEndTime != null);
        _appearance.SetData(ent, DiseaseDiagnoserVisuals.Vial, GetVialVisual(ent));
        _appearance.SetData(ent, DiseaseDiagnoserVisuals.Buffer, bufferFull);
    }

    private DiseaseDiagnoserVial GetVialVisual(Entity<DiseaseDiagnoserComponent> ent)
    {
        if (_itemSlots.GetItemOrNull(ent, ent.Comp.SlotId) is not { } item)
            return DiseaseDiagnoserVial.None;

        if (!_solutionContainer.TryGetFitsInDispenser(item, out _, out var solution) || solution.Volume <= FixedPoint2.Zero)
            return DiseaseDiagnoserVial.Empty;

        if (solution.GetTotalPrototypeQuantity(ent.Comp.MutagenReagent) > FixedPoint2.Zero
            || solution.GetTotalPrototypeQuantity(ent.Comp.UnstableMutagenReagent) > FixedPoint2.Zero)
            return DiseaseDiagnoserVial.Mutagen;

        return DiseaseDiagnoserVial.Blood;
    }

    private static void ClearResult(Entity<DiseaseDiagnoserComponent> ent)
    {
        ent.Comp.HasResult = false;
        ent.Comp.ResultVirusName = null;
        ent.Comp.ResultSymptoms = new();
        ent.Comp.ResultUnreadableCount = 0;
        ent.Comp.ResultTransmission = VirusTransmissionVector.None;
    }
}
