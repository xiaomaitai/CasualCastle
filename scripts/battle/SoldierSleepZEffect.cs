using Godot;

public partial class SoldierSleepZEffect : Node2D
{
	private Label _zSmall;
	private Label _zMedium;
	private Label _zLarge;
	private Vector2 _smallBase;
	private Vector2 _mediumBase;
	private Vector2 _largeBase;
	private float _time;

	public override void _Ready()
	{
		ZIndex = 12;

		_smallBase = new Vector2(6, -24);
		_mediumBase = new Vector2(14, -34);
		_largeBase = new Vector2(24, -46);

		_zSmall = CreateZLabel("z", 11, _smallBase);
		_zMedium = CreateZLabel("Z", 15, _mediumBase);
		_zLarge = CreateZLabel("Z", 20, _largeBase);

		Visible = false;
		SetProcess(false);
	}

	public void SetSleeping(bool sleeping)
	{
		Visible = sleeping;
		SetProcess(sleeping);
		if (!sleeping)
			_time = 0f;
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		AnimateLabel(_zSmall, _smallBase, 0f);
		AnimateLabel(_zMedium, _mediumBase, 0.35f);
		AnimateLabel(_zLarge, _largeBase, 0.7f);
	}

	private Label CreateZLabel(string text, int fontSize, Vector2 position)
	{
		var label = new Label
		{
			Text = text,
			Position = position,
		};
		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.Modulate = new Color(0.82f, 0.9f, 1f, 0.85f);
		AddChild(label);
		return label;
	}

	private void AnimateLabel(Label label, Vector2 basePos, float phaseOffset)
	{
		float phase = _time * 1.4f + phaseOffset;
		float bob = Mathf.Sin(phase) * 2f;
		float alpha = 0.5f + 0.4f * (0.5f + 0.5f * Mathf.Sin(phase * 1.8f));

		label.Position = new Vector2(basePos.X, basePos.Y - bob);
		label.Modulate = new Color(0.82f, 0.9f, 1f, alpha);
	}
}
