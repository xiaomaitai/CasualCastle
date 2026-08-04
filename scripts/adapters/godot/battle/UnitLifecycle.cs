// 单位死亡表现：尸体画面留存 5 秒后淡出并销毁节点
using Godot;

public class UnitLifecycle
{
	private const float CorpseHoldSeconds = 4.7f;
	private const float CorpseFadeSeconds = 0.3f;

	public void PlayDeathAnimation(Node2D owner)
	{
		Tween tween = owner.CreateTween();
		tween.TweenInterval(CorpseHoldSeconds);
		tween.TweenProperty(owner, "scale", Vector2.Zero, CorpseFadeSeconds);
		tween.Parallel().TweenProperty(owner, "modulate:a", 0f, CorpseFadeSeconds);
		tween.TweenCallback(Callable.From(owner.QueueFree));
	}
}
