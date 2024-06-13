// Licence 
using System.Numerics;
using Content.Shared.SS220.Thermals; // change 
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.SS220.Thermals;

public sealed class ThermalVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private readonly ContainerSystem _container;
    private readonly TransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public ThermalVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entity.TryGetComponent(_players.LocalEntity, out ThermalComponent? thermalVision) ||
            thermalVision.State == ThermalVisionState.Off)
        {
            return;
        }

        var handle = args.WorldHandle;
        var eye = args.Viewport.Eye;
        var eyeRot = eye?.Rotation ?? default;

        // TODO this should use its own component probably
        var entities = _entity.EntityQueryEnumerator<MobStateComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            Render((uid, sprite, xform), eye?.Position.MapId, handle, eyeRot);
        }

        handle.SetTransform(Matrix3.Identity);
    }

    private void Render(Entity<SpriteComponent, TransformComponent> ent, MapId? map, DrawingHandleWorld handle, Angle eyeRot)
    {
        var (uid, sprite, xform) = ent;
        if (xform.MapID != map)
            return;

        if (_container.IsEntityOrParentInContainer(uid))
            return;

        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        sprite.Render(handle, eyeRot, rotation, position: position);
    }
}
