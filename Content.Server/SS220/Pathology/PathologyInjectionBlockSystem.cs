// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Chemistry.Events;
using Content.Shared.SS220.Pathology;

namespace Content.Server.SS220.Pathology;

/// <summary>
/// Blocks injections (hypospray/syringe/medipen, all routed through <see cref="TargetBeforeInjectEvent"/>)
/// into a host carrying a <see cref="PathologyInjectionBlockComponent"/>, with its message.
/// </summary>
public sealed class PathologyInjectionBlockSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PathologyInjectionBlockComponent, TargetBeforeInjectEvent>(OnBeforeInject);
    }

    private void OnBeforeInject(Entity<PathologyInjectionBlockComponent> ent, ref TargetBeforeInjectEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancel();
        args.OverrideMessage = Loc.GetString(ent.Comp.Message);
    }
}
