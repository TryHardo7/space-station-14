using System.Text.RegularExpressions;
using Robust.Shared.Random;
using Content.Server.SS220.Speech.Components;
using Content.Server.Speech;

namespace Content.server.ss220.Speech.EntitySystems;

public sealed class VulpkaninAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VulpkaninAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, VulpkaninAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // р в рр или ррр
        message = Regex.Replace(
            message,
            "р+",
            _random.Pick(new List<string> { "рр", "ррр" })
        );
        // Р в РР или РРР 
        message = Regex.Replace(
            message,
            "Р+",
            _random.Pick(new List<string> { "РР", "РРР" })
        );
        // r into rrr
        message = Regex.Replace(
            message,
            "r+",
            _random.Pick(new List<string> { "rr", "rrr" })
        );
        // R into RRR
        message = Regex.Replace(
            message,
            "R+",
            _random.Pick(new List<string> { "RR", "RRR" })
        );

        args.Message = message;
    }
}
