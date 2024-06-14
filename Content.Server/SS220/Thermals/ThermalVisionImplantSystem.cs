using Content.Shared.Implants.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.SS220.Thermals;
using Content.Shared.Implants;
using Content.Shared.Actions;
using Robust.Shared.Timing;


namespace Content.Server.SS220.Thermals;

/// <summary>
/// Handles enabling of thermal vision when clothing is equipped and disabling when unequipped.
/// </summary>
public sealed class SharedThermalVisionImplantSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionImplantComponent, UseThermalVisionEvent>(OnThermalVisionAction);
    }

    private void OnThermalVisionAction(Entity<ThermalVisionImplantComponent> ent, ref UseThermalVisionEvent args)
    {
        if (TryComp<ThermalVisionImplantComponent>(args.Performer, out var thermalVision))
        {
            if (HasComp<ThermalComponent>(args.Performer) && thermalVision.IsAcive)
                RemComp<ThermalComponent>(args.Performer);
            else
                EnsureComp<ThermalComponent>(args.Performer);

            thermalVision.IsAcive = !thermalVision.IsAcive;
        }
    }
}

// что за ивент? зачем переменная вкл выкл компонент когда у нас идет присвоение и удаление компонента? 
// функция работает в рамках: (ThermalVisionImplantComponent и UseThermalVisionEvent)
// при активации экшена if args.performer HasComp<ThermalVisionImplantComponent> и его переменная Is.Active = false
//                   то ensure.Comp<ThermalComponent> на (args.Performer)
//  и сменить состояние Is.Active = true 
//                 else RemComp<ThermalComponent> на (args.Performer)
// и сменить состояние  Is.Active = false
// 
// либо функция работает в рамках (ThermalVisionImplantComponent и IsImplantImplanted и его args)