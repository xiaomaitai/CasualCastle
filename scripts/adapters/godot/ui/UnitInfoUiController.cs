// 单位信息组件：悬停详情（unit_info_panel.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class UnitInfoUiController : Control
{
	private Panel _panel;
	private Label _label;
	private bool _suppressed;
	private Node2D _battlefield;
	private readonly float _pickRadiusPixels = 40f;

	public override void _Ready()
	{
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
		AddChild(_panel);
	}

	public void SetSuppressed(bool suppressed)
	{
		_suppressed = suppressed;
		if (_suppressed)
			_panel.Visible = false;
	}

	public void Process()
	{
		if (_suppressed)
			return;
		Vector2 mouseGlobal = GetViewport().GetMousePosition();
		UnitLogic unit = PickUnit(mouseGlobal);

		if (unit == null || !unit.IsAlive)
		{
			_panel.Visible = false;
			return;
		}

		IUnitState svc = unit.Handle;
		if (svc == null)
		{
			_panel.Visible = false;
			return;
		}

		string side = unit.IsPlayerUnit ? "我方" : "敌方";
		string stateText = svc.State switch
		{
			UnitState.Marching => "行军",
			UnitState.Fighting => "战斗",
			UnitState.Retaliating => "反击",
			UnitState.Sieging => "攻城",
			UnitState.Healing => "治疗",
			_ => "未知"
		};
		string pos = $"位置: ({svc.GameX:F0}, {svc.GameY:F0})";
		string target = $"目标: {svc.TargetDescription}";

		_label.Text = $"{side} {stateText} | HP:{svc.Health}/{unit.MaxHealth}\n{pos}\n{target}";
		_panel.Position = mouseGlobal + new Vector2(16, 16);
		_panel.Visible = true;
	}

	private UnitLogic PickUnit(Vector2 globalPoint)
	{
		if (_battlefield == null)
		{
			Node mainGame = GetParent().GetParent();
			_battlefield = mainGame?.GetNodeOrNull<Node2D>("Battlefield");
			if (_battlefield == null)
				return null;
		}

		UnitLogic closest = null;
		float closestDist = _pickRadiusPixels;

		foreach (Node child in _battlefield.GetChildren())
		{
			Area2D area = child as Area2D;
			if (area == null)
				continue;

			UnitLogic logic = area.GetNodeOrNull<UnitLogic>("Logic");
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
