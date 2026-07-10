using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public enum UnitCardStatus
{
	None,
	Hit,
	Sleeping,
}

public partial class UnitCardView : Node2D
{
	private const float CardSize = 100f;
	private static readonly Color FriendlyColor = new Color(0.12f, 0.48f, 1f);
	private static readonly Color EnemyColor = new Color(0.92f, 0.16f, 0.18f);
	private static readonly Color SelectionColor = new Color(1f, 0.88f, 0.42f);

	private Sprite2D _portrait;
	private Label _nameLabel;
	private Label _statusLabel;
	private Label _buffLabel;
	private float _healthRatio = 1f;
	private bool _isPlayerUnit;
	private bool _selected;
	private UnitCardStatus _status;
	private string _typeId;
	private uint _unitColor;
	private float _displaySize;
	private bool _configured;

	public override void _Ready()
	{
		_portrait = GetNode<Sprite2D>("Portrait");
		_nameLabel = GetNode<Label>("NameLabel");
		_statusLabel = GetNode<Label>("StatusLabel");
		_buffLabel = GetNode<Label>("BuffLabel");
		if (_configured)
			ApplyConfiguration();
		UpdateStatusLabel();
	}

	public void Configure(string typeId, bool isPlayerUnit, uint unitColor, float displaySize)
	{
		_typeId = typeId;
		_isPlayerUnit = isPlayerUnit;
		_unitColor = unitColor;
		_displaySize = displaySize;
		_configured = true;
		if (IsNodeReady())
			ApplyConfiguration();
		QueueRedraw();
	}

	public void SetHealth(int current, int maximum)
	{
		_healthRatio = Mathf.Clamp(current / (float)maximum, 0f, 1f);
		QueueRedraw();
	}

	public void SetSelected(bool selected)
	{
		_selected = selected;
		QueueRedraw();
	}

	public void SetStatus(UnitCardStatus status)
	{
		_status = status;
		if (IsNodeReady())
			UpdateStatusLabel();
		QueueRedraw();
	}

	public void SetDimmed(bool dimmed)
	{
		Modulate = dimmed
			? new Color(0.66f, 0.72f, 0.86f, 0.88f)
			: Colors.White;
	}

	public void SetPortraitTint(Color color)
	{
		_portrait.Modulate = color;
	}

	public void SetBuffs(IReadOnlyList<string> buffs)
	{
		if (buffs.Count == 0)
		{
			_buffLabel.Visible = false;
			return;
		}

		StringBuilder text = new StringBuilder();
		int visibleBuffCount = Math.Min(buffs.Count, 4);
		for (int i = 0; i < visibleBuffCount; i++)
		{
			if (i > 0)
				text.Append(' ');

			if (buffs.Count > 4 && i == 3)
				text.Append($"+{buffs.Count - 3}");
			else
				text.Append(buffs[i]);
		}

		_buffLabel.Text = text.ToString();
		_buffLabel.Visible = true;
	}

	public override void _Draw()
	{
		if (_selected)
			DrawPanel(new Rect2(-55f, -105f, 110f, 110f), Colors.Transparent, SelectionColor, 3, 12);

		DrawPanel(new Rect2(-50f, -100f, 100f, 100f), new Color(0.08f, 0.09f, 0.11f), new Color(0.02f, 0.02f, 0.025f), 4, 10);

		Color factionColor = _isPlayerUnit ? FriendlyColor : EnemyColor;
		DrawPanel(new Rect2(-46f, -96f, 92f, 92f), new Color(0.12f, 0.13f, 0.15f), factionColor, 2, 8);
		DrawPanel(new Rect2(-42f, -92f, 84f, 84f), new Color(1f, 0.82f, 0.45f), new Color(1f, 0.93f, 0.68f), 2, 6);

		DrawPanel(new Rect2(-40f, -75f, 80f, 65f), new Color(1f, 0.78f, 0.35f), new Color(0.45f, 0.3f, 0.12f), 1, 4);
		DrawLine(new Vector2(-41f, -76f), new Vector2(41f, -76f), new Color(0.08f, 0.08f, 0.08f), 2f);

		DrawPanel(new Rect2(-40f, -90f, 80f, 13f), new Color(0.12f, 0.16f, 0.1f), new Color(0.04f, 0.05f, 0.04f), 1, 4);
		if (_healthRatio > 0f)
			DrawPanel(new Rect2(-39f, -89f, 78f * _healthRatio, 11f), GetHealthColor(), Colors.Transparent, 0, 3);

		if (_status != UnitCardStatus.None)
		{
			Color statusColor = _status == UnitCardStatus.Sleeping
				? new Color(0.2f, 0.48f, 0.95f)
				: new Color(0.92f, 0.2f, 0.18f);
			DrawCircle(new Vector2(31f, -65f), 8f, statusColor);
			DrawArc(new Vector2(31f, -65f), 8f, 0f, Mathf.Tau, 24, new Color(0.08f, 0.08f, 0.1f), 2f);
		}
	}

	private void UpdateStatusLabel()
	{
		_statusLabel.Text = _status switch
		{
			UnitCardStatus.Hit => "!",
			UnitCardStatus.Sleeping => "Z",
			_ => string.Empty,
		};
		_statusLabel.Visible = _status != UnitCardStatus.None;
	}

	private void ApplyConfiguration()
	{
		_nameLabel.Text = GetDisplayName(_typeId);
		_portrait.Modulate = ToColor(_unitColor);
		Scale = Vector2.One * Mathf.Max(_displaySize, 72f) / CardSize;
	}

	private Color GetHealthColor()
	{
		if (_healthRatio > 0.6f)
			return new Color(0.24f, 0.82f, 0.12f);
		if (_healthRatio > 0.3f)
			return new Color(1f, 0.78f, 0.1f);
		return new Color(0.94f, 0.18f, 0.14f);
	}

	private void DrawPanel(Rect2 rect, Color background, Color border, int borderWidth, int cornerRadius)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = background;
		style.BorderColor = border;
		style.SetBorderWidthAll(borderWidth);
		style.SetCornerRadiusAll(cornerRadius);
		DrawStyleBox(style, rect);
	}

	private static string GetDisplayName(string typeId)
	{
		return typeId switch
		{
			"Swordsman" => "剑士",
			"Archer" => "弓箭手",
			"Cavalry" => "骑兵",
			"Werewolf" => "狼人",
			"HeavySwordsman" => "重剑士",
			"WerewolfLord" => "狼人领主",
			_ => typeId,
		};
	}

	private static Color ToColor(uint value)
	{
		return new Color(
			((value >> 16) & 0xFF) / 255f,
			((value >> 8) & 0xFF) / 255f,
			(value & 0xFF) / 255f);
	}
}
