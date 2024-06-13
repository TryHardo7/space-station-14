using Robust.Shared.GameStates;

namespace Content.Shared.SS220.Thermals;

/// <summary>
/// Adds ThermalComponent to the user when implant action is used.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ThermalVisionImplantComponent : Component
{
[DataField, AutoNetworkedField]
    public bool IsAcive;
}