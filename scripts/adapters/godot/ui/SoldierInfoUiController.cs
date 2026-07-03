using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System.Collections.Generic;

public sealed class SoldierInfoUiController
{
	private readonly Node _owner;
	private readonly Panel _panel;
	private readonly Label _label;

	public SoldierInfoUiController(Node owner, CanvasLayer uiRoot)
	{
		_owner = owner;

		_panel = new Panel();
		_panel.Size = new Vector2(200, 80);
		_panel.Visible = false;
		_panel.MouseFilter = Control.MouseFilterEnum.Ignore;
		StyleBoxFlat bg = new StyleBoxFlat();
		bg.BgColor = new Color(0, 0, 0, 0.85f);
		bg.BorderColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
		bg.SetBorderWidthAll(1);
		bg.SetCornerRadiusAll(4);
		_panel.AddThemeStyleboxOverride("panel", bg);

		_label = new Label();
		_label.Modulate = Colors.White;
		_label.HorizontalAlignment = HorizontalAlignment.Left;
		_label.AddThemeFontSizeOverride("font_size", 12);
		_label.MouseFilter = Control.MouseFilterEnum.Ignore;
		_label.Position = new Vector2(4, 2);

		_panel.AddChild(_label);
		uiRoot.AddChild(_panel);
	}

	public void Process()
	{
		Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
		SoldierLogic soldier = PickSoldier(mouseGlobal);

		if (soldier == null || !soldier.IsAlive)
		{
			_panel.Visible = false;
			return;
		}

		ISoldierState svc = soldier.Handle;
		if (svc == null)
		{
			_panel.Visible = false;
			return;
		}

		string side = soldier.IsPlayerUnit ? "我方" : "敌方";
		string stateText = svc.State switch
		{
			SoldierState.Marching => "行军",
			SoldierState.Fighting => "战斗",
			SoldierState.Retaliating => "反击",
			SoldierState.Sieging => "攻城",
			_ => "未知"
		};
		string pos = $"位置: ({svc.GameX:F0}, {svc.GameY:F0})";
		string target = $"目标: {svc.TargetDescription}";

		_label.Text = $"{side} {stateText} | HP:{svc.Health}/{soldier.MaxHealth}\n{pos}\n{target}";
		_panel.Position = mouseGlobal + new Vector2(16, 16);
		_panel.Visible = true;
	}

	private Node2D _battlefield;
	private float _pickRadiusPixels = 40f;

	private SoldierLogic PickSoldier(Vector2 globalPoint)
	{
		if (_battlefield == null)
		{
			Node mainGame = _owner.GetParent();
			_battlefield = mainGame?.GetNodeOrNull<Node2D>("Battlefield");
			if (_battlefield == null)
				return null;
		}

		SoldierLogic closest = null;
		float closestDist = _pickRadiusPixels;

		foreach (Node child in _battlefield.GetChildren())
		{
			Area2D area = child as Area2D;
			if (area == null)
				continue;

			SoldierLogic logic = area.GetNodeOrNull<SoldierLogic>("Logic");
			if (logic == null || !logic.IsAlive)
				continue;

			float dist = area.GlobalPosition.DistanceTo(globalPoint);
			if (dist < closestDist)
			{
				closestDist = dist;
				closest = logic;
			}
		}

		return closest;
	}
}
