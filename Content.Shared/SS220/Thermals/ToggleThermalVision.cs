// Licence
using Content.Shared.Alert;

namespace Content.Shared.SS220.Thermals;

[DataDefinition]
public sealed partial class ToggleThermalVision : IAlertClick  // what is IAlertClick - is it eye on UI?
{
    public void AlertClicked(EntityUid player)
    {
        var entities = IoCManager.Resolve<IEntityManager>();
        entities.System<SharedThermalSystem>().Toggle(player);
    }
}
