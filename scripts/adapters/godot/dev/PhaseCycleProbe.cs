// 临时诊断脚本：驱动完整阶段循环，观察逻辑阶段与计时
using System.Collections.Generic;
using CasualCastle.Adapters.Godot;
using Godot;

public partial class PhaseCycleProbe : Node
{
	private float _elapsed;
	private float _lastLog;
	private bool _advancedShop;
	private bool _advancedCombine;

	public override void _Ready()
	{
		AddChild(GD.Load<PackedScene>("res://scenes/main/main_game.tscn").Instantiate());
	}

	public override void _Process(double delta)
	{
		_elapsed += (float)delta;
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		if (gm == null || gm.PlayerCastle == null)
			return;

		if (_elapsed - _lastLog >= 2f)
		{
			_lastLog = _elapsed;
			GD.Print($"PROBE t={_elapsed:F0} phase={gm.CurrentPhase} timer={gm.PhaseTimeRemaining:F1} night={gm.CurrentNightIndex}");
		}

		if (gm.CurrentPhase == GameManager.GamePhase.Shop && !_advancedShop)
		{
			_advancedShop = true;
			GD.Print("PROBE advance from shop");
			gm.AdvancePhase();
		}
		else if (gm.CurrentPhase == GameManager.GamePhase.Combine && !_advancedCombine)
		{
			_advancedCombine = true;
			GD.Print("PROBE advance from combine");
			gm.AdvancePhase();
		}
		else if (gm.CurrentPhase == GameManager.GamePhase.Combat && _advancedCombine && _elapsed > 70f)
		{
			GD.Print("PROBE full cycle ok");
			GetTree().Quit(0);
		}
	}
}
