
using Content.Shared.Actions;
using Content.Shared.SS220.XenoLanc;

namespace Content.Shared.SS220.XenoLanc;

public sealed partial class XenoHealEvent : EntityTargetActionEvent;

public override void Initialize()

SubscribeLocalEvent<XenoActionsComponent, XenoHealEvent(HealAction)>
