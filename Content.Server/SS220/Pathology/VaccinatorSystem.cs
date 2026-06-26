// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using System.Collections.Generic;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Server.Power.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.SS220.Pathology;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.SS220.Pathology;

public sealed partial class VaccinatorSystem : EntitySystem
{
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private SharedPathologySystem _pathology = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    private static readonly TimeSpan UiUpdateInterval = TimeSpan.FromSeconds(0.2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VaccinatorComponent, ComponentStartup>(OnUiDirty);
        SubscribeLocalEvent<VaccinatorComponent, BoundUIOpenedEvent>(OnUiDirty);
        SubscribeLocalEvent<VaccinatorComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<VaccinatorComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<VaccinatorComponent, VaccinatorScanMessage>(OnScan);
        SubscribeLocalEvent<VaccinatorComponent, VaccinatorTransferMessage>(OnTransfer);
        SubscribeLocalEvent<VaccinatorComponent, VaccinatorCreateVaccineMessage>(OnCreateVaccine);
    }

    private void OnUiDirty<T>(Entity<VaccinatorComponent> ent, ref T args)
    {
        UpdateUi(ent);
    }

    private void OnEntInserted(Entity<VaccinatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        OnSlotChanged(ent, args.Container.ID);
    }

    private void OnEntRemoved(Entity<VaccinatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        OnSlotChanged(ent, args.Container.ID);
    }

    private void OnSlotChanged(Entity<VaccinatorComponent> ent, string containerId)
    {
        if (containerId != ent.Comp.SlotId)
            return;

        ent.Comp.ScanEndTime = null;
        ClearResult(ent);
        UpdateUi(ent);
    }

    private void OnScan(Entity<VaccinatorComponent> ent, ref VaccinatorScanMessage args)
    {
        // the UI stays open after power is cut, so gate the action itself, not just opening
        if (!this.IsPowered(ent, EntityManager))
            return;

        if (ent.Comp.ScanEndTime != null || _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId) == null)
            return;

        ClearResult(ent);
        ent.Comp.ScanEndTime = _timing.CurTime + ent.Comp.ScanDuration;
        ent.Comp.NextUiUpdate = _timing.CurTime;
        UpdateUi(ent);
    }

    private void OnTransfer(Entity<VaccinatorComponent> ent, ref VaccinatorTransferMessage args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        var item = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId);
        if (item == null
            || !_solutionContainer.TryGetFitsInDispenser(item.Value, out var sourceSoln, out var source)
            || !_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out var bufferSoln, out var buffer))
            return;

        var available = source.GetTotalPrototypeQuantity(ent.Comp.TricordrazineReagent);
        var amount = FixedPoint2.Min(available, buffer.MaxVolume - buffer.Volume);
        if (amount <= FixedPoint2.Zero)
        {
            _popup.PopupEntity(Loc.GetString("vaccinator-no-tricordrazine-source"), ent, args.Actor);
            return;
        }

        _solutionContainer.RemoveReagent(sourceSoln.Value, ent.Comp.TricordrazineReagent, amount);
        _solutionContainer.TryAddReagent(bufferSoln.Value, ent.Comp.TricordrazineReagent, amount, out _);
        UpdateUi(ent);
    }

    private void OnCreateVaccine(Entity<VaccinatorComponent> ent, ref VaccinatorCreateVaccineMessage args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        var item = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId);
        if (item == null || !_solutionContainer.TryGetFitsInDispenser(item.Value, out _, out var source))
            return;
        var strains = new HashSet<string>();

        foreach (var virus in VirusData.EnumerateViruses(source))
        {
            if (virus.SuppressedUntil is { } until && _timing.CurTime < until)
                strains.Add(_pathology.GetIdentity(virus));
        }

        if (strains.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("vaccinator-no-cured-blood"), ent, args.Actor);
            return;
        }

        if (!_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out var bufferSoln, out var buffer)
            || buffer.GetTotalPrototypeQuantity(ent.Comp.TricordrazineReagent) < ent.Comp.VaccineAmount)
        {
            _popup.PopupEntity(Loc.GetString("vaccinator-no-tricordrazine"), ent, args.Actor);
            return;
        }

        var bottle = Spawn(ent.Comp.VaccineBottle, Transform(ent).Coordinates);
        if (!_solutionContainer.TryGetSolution(bottle, ent.Comp.BottleSolutionId, out var bottleSoln, out _))
        {
            Log.Error($"VaccineBottle {ent.Comp.VaccineBottle} has no '{ent.Comp.BottleSolutionId}' solution");
            Del(bottle);
            return;
        }

        _solutionContainer.RemoveReagent(bufferSoln.Value, ent.Comp.TricordrazineReagent, ent.Comp.VaccineAmount);

        var vaccineData = new VaccineData { Strains = new List<string>(strains) };
        _solutionContainer.TryAddReagent(bottleSoln.Value, ent.Comp.VaccineReagent, ent.Comp.VaccineAmount, out _, data: new List<ReagentData> { vaccineData });

        _popup.PopupEntity(Loc.GetString("vaccinator-vaccine-created"), ent, args.Actor);
        UpdateUi(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<VaccinatorComponent>();
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

    private void BuildResult(Entity<VaccinatorComponent> ent)
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

                if (readable)
                    ent.Comp.ResultSymptoms.Add(_pathology.FormatSymptom(symptomId, description!, virus));
                else
                    ent.Comp.ResultUnreadableCount++;
            }

            if (allReadable
                && ent.Comp.ResultVirusName == null
                && virus.Name is { } name)
            {
                ent.Comp.ResultVirusName = name;
            }

            if (virus.Cure is not { } cure)
                continue;

            // the cure reagents only show once every symptom of the strain has been revealed
            if (allReadable)
            {
                foreach (var reagent in cure.Reagents)
                {
                    if (_prototype.TryIndex<ReagentPrototype>(reagent, out var reagentProto))
                        ent.Comp.ResultCureReagents.Add(reagentProto.LocalizedName);
                }
            }
            else
            {
                ent.Comp.ResultCureHidden = true;
            }
        }
    }

    private void UpdateUi(Entity<VaccinatorComponent> ent)
    {
        var hasSample = _itemSlots.GetItemOrNull(ent, ent.Comp.SlotId) != null;
        var scanning = ent.Comp.ScanEndTime != null;
        var progress = PathologyMachine.ComputeScanProgress(ent.Comp.ScanEndTime, ent.Comp.ScanDuration, ent.Comp.HasResult, _timing.CurTime);

        var bufferTricordrazine = 0f;
        if (_solutionContainer.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionId, out _, out var buffer))
            bufferTricordrazine = (float)buffer.GetTotalPrototypeQuantity(ent.Comp.TricordrazineReagent);

        var state = new VaccinatorBoundUserInterfaceState(
            hasSample,
            scanning,
            progress,
            ent.Comp.HasResult,
            ent.Comp.ResultVirusName,
            new List<string>(ent.Comp.ResultSymptoms),
            ent.Comp.ResultUnreadableCount,
            new List<string>(ent.Comp.ResultCureReagents),
            ent.Comp.ResultCureHidden,
            bufferTricordrazine);

        _ui.SetUiState(ent.Owner, VaccinatorUiKey.Key, state);
    }

    private static void ClearResult(Entity<VaccinatorComponent> ent)
    {
        ent.Comp.HasResult = false;
        ent.Comp.ResultVirusName = null;
        ent.Comp.ResultSymptoms = new();
        ent.Comp.ResultUnreadableCount = 0;
        ent.Comp.ResultCureReagents = new();
        ent.Comp.ResultCureHidden = false;
    }
}
