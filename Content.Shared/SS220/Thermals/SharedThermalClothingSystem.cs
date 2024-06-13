using Content.Shared.Inventory.Events;
using Content.Shared.SS220.Thermals;
using Robust.Shared.Timing;


namespace Content.Server.SS220.Thermals;

/// <summary>
/// Handles enabling of thermal vision when clothing is equipped and disabling when unequipped.
/// </summary>
public sealed class SharedThermalClothingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalClothingComponent, GotEquippedEvent>(OnCompEquip);
        SubscribeLocalEvent<ThermalClothingComponent, GotUnequippedEvent>(OnCompUnequip);
    }
    private void OnCompEquip(Entity<ThermalClothingComponent> ent, ref GotEquippedEvent args)
    {
        if (!HasComp<ThermalComponent>(args.Equipee) && !_gameTiming.ApplyingState)
            EnsureComp<ThermalComponent>(args.Equipee);
    }

    private void OnCompUnequip(Entity<ThermalClothingComponent> ent, ref GotUnequippedEvent args)
    {
        if (HasComp<ThermalComponent>(args.Equipee))
            RemComp<ThermalComponent>(args.Equipee);
    }

}