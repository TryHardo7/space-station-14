using Content.Shared.Implants.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.SS220.Thermals;
using Content.Shared.Implants;
using Robust.Shared.Timing;


namespace Content.Server.SS220.Thermals;

/// <summary>
/// Handles enabling of thermal vision when clothing is equipped and disabling when unequipped.
/// </summary>
public sealed class SharedThermalVisionImplantSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionImplantComponent, UseThermalVisionEvent>(OnThermalVisionImplant);
    }

    private void OnThermalVisionImplant(Entity<ThermalVisionImplantComponent> ent, ref UseThermalVisionEvent args)
    {
        if (TryComp<ThermalVisionImplantComponent>(args.Performer, out var thermalVision) && !_gameTiming.ApplyingState)
        {
            if (thermalVision.IsAcive)
            {
                RemComp<ThermalComponent>(args.Performer);
                thermalVision.IsAcive = false;
            }
            else
            {
                EnsureComp<ThermalComponent>(args.Performer);
                thermalVision.IsAcive = true;
            }
        }

    }
}
