using Godot;
using System;

public class SoldierLifecycle
{
    public void PlayDeathAnimation(Node2D owner, Action onCleanup)
    {
        Tween tween = owner.CreateTween();
        tween.TweenProperty(owner, "scale", Vector2.Zero, 0.25f);
        tween.Parallel().TweenProperty(owner, "modulate:a", 0f, 0.25f);
        tween.TweenCallback(Callable.From(() =>
        {
            onCleanup();
            owner.QueueFree();
        }));
    }
}
