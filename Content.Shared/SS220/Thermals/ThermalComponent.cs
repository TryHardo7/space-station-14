// Licence 
using Content.Shared.Actions;
using Content.Shared.Alert;
using Linguini.Syntax.Ast;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Thermals;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ThermalComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> Alert = "ThermalVision";  // "XenoNightVision";

    [DataField, AutoNetworkedField]
    public ThermalVisionState State = ThermalVisionState.Half; // need to think it through - states? 
}

[Serializable, NetSerializable]
public enum ThermalVisionState   // change it 
{
    Off,
    Half,
    Full
}

public sealed partial class UseThermalVisionEvent : InstantActionEvent
{

}