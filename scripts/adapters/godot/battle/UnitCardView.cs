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
	private const float CardSpan = 1132f;
	private const float StatusIndicatorCx = 340f;
	private const float StatusIndicatorCy = -420f;
	private const float StatusIndicatorR = 60f;
	private const float StatusIndicatorRingR = 72f;

	private Sprite2D _cardBase;
	private HealthBarView _healthBar;
	private CardArtView _cardArt;
	private Label _nameLabel;
	private Label _statusLabel;
	private Label _buffLabel;
	private float _healthRatio = 1f;
	private bool _selected;
	private UnitCardStatus _status;
	private string _typeId;
	private float _displaySize;
	private bool _configured;

	public override void _Ready()
	{
		_cardBase = GetNode<Sprite2D>("CardBase");
		_healthBar = GetNode<HealthBarView>("HealthBar");
		_cardArt = GetNode<CardArtView>("CardArt");
		_nameLabel = GetNode<Label>("NameLabel");
		_statusLabel = GetNode<Label>("StatusLabel");
		_buffLabel = GetNode<Label>("BuffLabel");
		if (_configured)
			ApplyConfiguration();
		UpdateStatusLabel();
	}

	public void Configure(string typeId, float displaySize)
	{
		_typeId = typeId;
		_displaySize = displaySize;
		_configured = true;
		if (IsNodeReady())
			ApplyConfiguration();
		QueueRedraw();
	}

	public void SetHealth(int current, int maximum)
	{
		_healthRatio = Mathf.Clamp(current / (float)maximum, 0f, 1f);
		if (IsNodeReady())
			_healthBar.Fill = _healthRatio;
	}

	public void SetSelected(bool selected)
	{
		_selected = selected;
		ZIndex = selected ? 20 : 0;
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
		_cardArt.SetPortraitTint(color);
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
		if (_status == UnitCardStatus.None)
			return;

		Color fill = _status == UnitCardStatus.Sleeping
			? new Color(0.2f, 0.48f, 0.95f)
			: new Color(0.92f, 0.2f, 0.18f);
		DrawCircle(new Vector2(StatusIndicatorCx, StatusIndicatorCy), StatusIndicatorR, fill);
		DrawArc(new Vector2(StatusIndicatorCx, StatusIndicatorCy), StatusIndicatorRingR, 0f, Mathf.Tau, 32, new Color(0.06f, 0.07f, 0.1f), 16f);
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
		_cardArt.SetPortraitTint(Colors.White);
		Scale = Vector2.One * Mathf.Max(_displaySize, 72f) / CardSpan;
		_healthBar.Fill = _healthRatio;
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
}
