// Licence
using Content.Shared.Alert;
using Content.Shared.Rounding;

namespace Content.Shared.SS220.Thermals;

public abstract class SharedThermalSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ThermalComponent, ComponentStartup>(OnThermalStartup);
        SubscribeLocalEvent<ThermalComponent, MapInitEvent>(OnThermalMapInit);
        SubscribeLocalEvent<ThermalComponent, AfterAutoHandleStateEvent>(OnThermalHandle); //what 
        SubscribeLocalEvent<ThermalComponent, ComponentRemove>(OnThermalRemove);
    }

    private void OnThermalStartup(Entity<ThermalComponent> ent, ref ComponentStartup args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalHandle(Entity<ThermalComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalMapInit(Entity<ThermalComponent> ent, ref MapInitEvent args)
    {
    //    UpdateAlert(ent);
    }

    private void OnThermalRemove(Entity<ThermalComponent> ent, ref ComponentRemove args)
    {
    //    _alerts.ClearAlert(ent, ent.Comp.Alert); 
       ThermalVisionRemoved(ent);
    }

    public void Toggle(Entity<ThermalComponent?> ent) //remove
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.State = ent.Comp.State switch
        {
            ThermalVisionState.Off => ThermalVisionState.Half,
            ThermalVisionState.Half => ThermalVisionState.Full,
            ThermalVisionState.Full => ThermalVisionState.Off,
            _ => throw new ArgumentOutOfRangeException() 
        };

        Dirty(ent);
    //   UpdateAlert((ent, ent.Comp));
    }

    //private void UpdateAlert(Entity<ThermalComponent> ent)
    //{
    //    var level = MathF.Max((int) ThermalVisionState.Off, (int) ent.Comp.State);
    //    var max = _alerts.GetMaxSeverity(ent.Comp.Alert);
    //    var severity = max - ContentHelpers.RoundToLevels(level, (int) ThermalVisionState.Full, max + 1);
    //   _alerts.ShowAlert(ent, ent.Comp.Alert, (short) severity); 

    //    ThermalVisionChanged(ent);
    //}

    protected virtual void ThermalVisionChanged(Entity<ThermalComponent> ent)
    {
    }

    protected virtual void ThermalVisionRemoved(Entity<ThermalComponent> ent)
    {
    }
}
