using Content.Shared.Actions;


namespace Content.Shared.SS220.XenoLanc;
/// <summary>
/// This is used for custom xeno actions.
/// </summary>

[RegisterComponent]
public sealed partial class XenoActionsComponent : Component
{
    [DataField(required: true)]
}
public sealed partial class XenoStealthEvent : InstantActionEvent { };
