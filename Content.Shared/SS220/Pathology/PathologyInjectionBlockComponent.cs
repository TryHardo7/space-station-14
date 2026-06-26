// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

namespace Content.Shared.SS220.Pathology;

/// <summary>
/// Generic symptom behaviour: hypospray/syringe/medipen can't inject the host, and whoever tries gets
/// the given message. Compose it onto any "can't be injected" symptom in YAML — no bespoke system.
/// </summary>
[RegisterComponent]
public sealed partial class PathologyInjectionBlockComponent : Component
{
    /// <summary>Localized message shown to whoever tries to inject the host.</summary>
    [DataField(required: true)]
    public LocId Message;
}
