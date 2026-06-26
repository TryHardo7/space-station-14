// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.SS220.Pathology.Effects;

/// <summary>Injects amount of a reagent into the host's bloodstream every update interval.</summary>
public sealed partial class PathologyInjectReagentEffect : IPathologyEffect
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent;

    [DataField]
    public FixedPoint2 Amount = FixedPoint2.New(1);

    public void ApplyEffect(in PathologyEffectArgs args)
    {
        var solution = new Solution(Reagent, Amount);
        args.EntityManager.System<SharedBloodstreamSystem>().TryAddToBloodstream(args.Target, solution);
    }
}
