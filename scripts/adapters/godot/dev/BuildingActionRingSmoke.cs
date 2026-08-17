// 建筑行动环冒烟：无头验证环按钮布局与卖出/暂停/升级/仪式交互
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;

public partial class BuildingActionRingSmoke : SmokeTestBase
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const float ButtonSize = 64f;
	private const float RingRadius = 150f;
	private const float SlotStep = Mathf.Pi / 6f;
	private const float PositionTolerance = 4f;
	private const int SettleFrames = 6;
	private const float TotalTimeout = 60f;
	private const int SellPriceTomb = 1;
	private const int UpgradeCostTomb = 3;

	private enum SmokeStep
	{
		SelectTomb,
		VerifyLayout,
		TogglePause,
		VerifyPause,
		SellTomb,
		VerifySell,
		UpgradeTomb,
		VerifyUpgrade,
		VerifyUpgraded,
		EnableConfirmSell,
		ClickSellWithConfirm,
		VerifyConfirmDialog,
		CancelConfirm,
		VerifyConfirmCanceled,
		SellWithConfirm,
		VerifyConfirmDialogAgain,
		AcceptConfirm,
		VerifyConfirmAccepted,
		SelectDarkAltar,
		VerifySpecial,
		ToggleSpecialHidden,
		VerifySpecialHidden,
		ToggleSpecialShown,
		VerifySpecialShown
	}

	private readonly List<string> _failures = new();
	private Castle _playerCastle;
	private Shop _shop;
	private BuildingActionRing _ring;
	private Control _spellBar;
	private Building _tomb;
	private Building _darkAltar;
	private Control _sellConfirmOverlay;
	private Button _sellConfirmOk;
	private Button _sellConfirmCancel;
	private SmokeStep _step;
	private int _settleFrames;
	private int _goldBefore;
	private float _elapsed;
	private Vector2 _inputScale = Vector2.One;
	private bool _inputScaleReady;

	public override void _Input(InputEvent @event)
	{
		if (_inputScaleReady || @event is not InputEventMouseButton mouse || !mouse.Pressed)
			return;
		_inputScale = mouse.GlobalPosition / Vector2.One;
		_inputScaleReady = true;
	}

	public override void _Ready()
	{
		StartWatchdog(180f);
		AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(FactionDef.NecromancerId);
		AddChild(GD.Load<PackedScene>(MainGameScene).Instantiate());
		_playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
		_shop = AdapterRegistry.Resolve<Shop>();
		_ring = RequireNode<BuildingActionRing>(this, "MainGame/UI/BuildingActionRing");
		_spellBar = RequireNode<Control>(this, "MainGame/UI/SpellBarPanel");
		_sellConfirmOverlay = RequireNode<Control>(this, "MainGame/UI/SellConfirmOverlay");
		_sellConfirmOk = RequireNode<Button>(this, "MainGame/UI/SellConfirmOverlay/SellConfirmPanel/ConfirmButton");
		_sellConfirmCancel = RequireNode<Button>(this, "MainGame/UI/SellConfirmOverlay/SellConfirmPanel/CancelButton");
		PlaceBuildings();
		_step = SmokeStep.SelectTomb;
	}

	public override void _Process(double delta)
	{
		_elapsed += (float)delta;
		if (_elapsed > TotalTimeout)
		{
			Fail("冒烟超时");
			ReportAndQuit();
			return;
		}
		RunGuarded(AdvanceStep);
	}

	private void PlaceBuildings()
	{
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (_playerCastle == null)
		{
			Fail("玩家城堡缺失");
			return;
		}
		if (system.TryPlace(_playerCastle, "Tomb", 0, 0))
			_tomb = FindBuilding("Tomb");
		else
			Fail("Tomb 放置失败");
		_darkAltar = system.TryPlace(_playerCastle, "DarkAltar", 0, 2) ? FindBuilding("DarkAltar") : null;
		if (_darkAltar == null)
			Fail("DarkAltar 放置失败");
		GameManager.Get<RitualService>().Learn(RitualRules.EvilRitualId);
	}

	private void AdvanceStep()
	{
		switch (_step)
		{
			case SmokeStep.SelectTomb:
				PushBuildingClick(_tomb);
				Wait(SmokeStep.VerifyLayout);
				break;
			case SmokeStep.VerifyLayout:
				VerifyTombLayout();
				break;
			case SmokeStep.TogglePause:
				PushButtonClick(GetButton(1));
				Wait(SmokeStep.VerifyPause);
				break;
			case SmokeStep.VerifyPause:
				VerifyPause();
				break;
			case SmokeStep.SellTomb:
				_goldBefore = _shop.Gold;
				PushButtonClick(GetButton(0));
				Wait(SmokeStep.VerifySell);
				break;
			case SmokeStep.VerifySell:
				VerifySell();
				break;
			case SmokeStep.UpgradeTomb:
				ReplaceTombForUpgrade();
				break;
			case SmokeStep.VerifyUpgrade:
				VerifyUpgradeButton();
				break;
			case SmokeStep.VerifyUpgraded:
				VerifyUpgraded();
				break;
			case SmokeStep.EnableConfirmSell:
				EnableConfirmSell();
				break;
			case SmokeStep.ClickSellWithConfirm:
				ClickSellWithConfirm();
				break;
			case SmokeStep.VerifyConfirmDialog:
				VerifyConfirmDialog();
				break;
			case SmokeStep.CancelConfirm:
				PushButtonClick(_sellConfirmCancel);
				Wait(SmokeStep.VerifyConfirmCanceled);
				break;
			case SmokeStep.VerifyConfirmCanceled:
				VerifyConfirmCanceled();
				break;
			case SmokeStep.SellWithConfirm:
				PushButtonClick(GetButton(0));
				Wait(SmokeStep.VerifyConfirmDialogAgain);
				break;
			case SmokeStep.VerifyConfirmDialogAgain:
				VerifyConfirmDialogAgain();
				break;
			case SmokeStep.AcceptConfirm:
				PushButtonClick(_sellConfirmOk);
				Wait(SmokeStep.VerifyConfirmAccepted);
				break;
			case SmokeStep.VerifyConfirmAccepted:
				VerifyConfirmAccepted();
				break;
			case SmokeStep.SelectDarkAltar:
				PushBuildingClick(_darkAltar);
				Wait(SmokeStep.VerifySpecial);
				break;
			case SmokeStep.VerifySpecial:
				VerifySpecialInitial();
				break;
			case SmokeStep.ToggleSpecialHidden:
				PushButtonClick(GetButton(3));
				Wait(SmokeStep.VerifySpecialHidden);
				break;
			case SmokeStep.VerifySpecialHidden:
				VerifySpecialHidden();
				break;
			case SmokeStep.ToggleSpecialShown:
				PushButtonClick(GetButton(3));
				Wait(SmokeStep.VerifySpecialShown);
				break;
			case SmokeStep.VerifySpecialShown:
				VerifySpecialShown();
				break;
		}
	}

	private void VerifyTombLayout()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (!_ring.Visible)
		{
			Fail($"选中后环未显示 position={_ring.Position}");
			ReportAndQuit();
			return;
		}
		if (_ring.GetChildCount() != 3)
		{
			Fail($"Tomb 按钮数量错误 count={_ring.GetChildCount()}");
			ReportAndQuit();
			return;
		}
		VerifyButton(0, 0f, "sell");
		VerifyButton(1, -SlotStep, "pause");
		VerifyButton(2, SlotStep, "upgrade");
		_step = SmokeStep.TogglePause;
	}

	private void VerifyButton(int index, float angle, string label)
	{
		Button button = _ring.GetChild<Button>(index);
		Vector2 expected = _ring.Position + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * RingRadius;
		Vector2 actual = button.GlobalPosition + new Vector2(ButtonSize * 0.5f, ButtonSize * 0.5f);
		if (actual.DistanceTo(expected) > PositionTolerance)
			Fail($"{label} 按钮位置错误 actual={actual} expected={expected}");
	}

	private void VerifyPause()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (!_tomb.IsManuallyPaused)
			Fail("暂停按钮未生效");
		if (!GetButton(1).ButtonPressed)
			Fail("暂停按钮按下态错误");
		_step = SmokeStep.SellTomb;
	}

	private void VerifySell()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (_shop.Gold != _goldBefore + SellPriceTomb)
			Fail($"卖出金币错误 before={_goldBefore} after={_shop.Gold}");
		if (FindBuilding("Tomb") != null)
			Fail("卖出后建筑仍在");
		if (_ring.Visible)
			Fail("卖出后环未隐藏");
		_step = SmokeStep.UpgradeTomb;
	}

	private void ReplaceTombForUpgrade()
	{
		_playerCastle.ReplaceHeart("RitualCircleL2", _playerCastle.Heart.MaxHealth);
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (!system.TryPlace(_playerCastle, "Tomb", 0, 0))
		{
			Fail("重新放置 Tomb 失败");
			ReportAndQuit();
			return;
		}
		_tomb = FindBuilding("Tomb");
		_goldBefore = _shop.Gold;
		PushBuildingClick(_tomb);
		Wait(SmokeStep.VerifyUpgrade);
	}

	private void VerifyUpgradeButton()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (_ring.GetChildCount() != 3)
			Fail($"升级按钮数量错误 count={_ring.GetChildCount()}");
		Button upgrade = GetButton(2);
		if (upgrade.Disabled)
			Fail("升级按钮不应禁用");
		PushButtonClick(upgrade);
		Wait(SmokeStep.VerifyUpgraded);
	}

	private void VerifyUpgraded()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (FindBuilding("Necropolis") == null)
			Fail("升级后未生成 Necropolis");
		if (FindBuilding("Tomb") != null)
			Fail("升级后 Tomb 仍存在");
		if (_shop.Gold != _goldBefore - UpgradeCostTomb)
			Fail($"升级金币错误 before={_goldBefore} after={_shop.Gold}");
		_step = SmokeStep.EnableConfirmSell;
	}

	private void EnableConfirmSell()
	{
		DisplaySettingsManager.ConfirmSellEnabled = true;
		BuildingSystem system = AdapterRegistry.Resolve<BuildingSystem>();
		if (!system.TryPlace(_playerCastle, "Tomb", 4, 0))
		{
			Fail("确认出售测试放置 Tomb 失败");
			ReportAndQuit();
			return;
		}
		_tomb = FindBuilding("Tomb");
		_goldBefore = _shop.Gold;
		PushBuildingClick(_tomb);
		Wait(SmokeStep.ClickSellWithConfirm);
	}

	private void ClickSellWithConfirm()
	{
		PushButtonClick(GetButton(0));
		Wait(SmokeStep.VerifyConfirmDialog);
	}

	private void VerifyConfirmDialog()
	{
		if (!_sellConfirmOverlay.Visible)
			Fail("确认出售弹窗未显示");
		if (FindBuilding("Tomb") == null)
			Fail("弹窗打开时建筑不应被出售");
		if (_shop.Gold != _goldBefore)
			Fail($"弹窗打开时金币不应变化 before={_goldBefore} after={_shop.Gold}");
		_step = SmokeStep.CancelConfirm;
	}

	private void VerifyConfirmCanceled()
	{
		if (_sellConfirmOverlay.Visible)
			Fail("取消后弹窗应关闭");
		if (FindBuilding("Tomb") == null)
			Fail("取消后建筑仍应存在");
		if (_shop.Gold != _goldBefore)
			Fail($"取消后金币不应变化 before={_goldBefore} after={_shop.Gold}");
		_step = SmokeStep.SellWithConfirm;
	}

	private void VerifyConfirmDialogAgain()
	{
		if (!_sellConfirmOverlay.Visible)
			Fail("再次点击出售弹窗未显示");
		_step = SmokeStep.AcceptConfirm;
	}

	private void VerifyConfirmAccepted()
	{
		if (_sellConfirmOverlay.Visible)
			Fail("确认后弹窗应关闭");
		if (FindBuilding("Tomb") != null)
			Fail("确认后建筑应被出售");
		if (_shop.Gold != _goldBefore + SellPriceTomb)
			Fail($"确认出售金币错误 before={_goldBefore} after={_shop.Gold}");
		DisplaySettingsManager.ConfirmSellEnabled = false;
		_step = SmokeStep.SelectDarkAltar;
	}

	private void VerifySpecialInitial()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (!_ring.Visible)
			Fail("选中仪式建筑时环未显示");
		if (_ring.GetChildCount() != 4)
			Fail($"仪式建筑按钮数量错误 count={_ring.GetChildCount()}");
		if (!_spellBar.Visible)
			Fail("选中仪式建筑时法术栏应自动显示");
		Button special = GetButton(3);
		if (!special.ButtonPressed)
			Fail("仪式按钮应处于按下态");
		_step = SmokeStep.ToggleSpecialHidden;
	}

	private void VerifySpecialHidden()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (_spellBar.Visible)
			Fail("点击仪式按钮后法术栏应隐藏");
		_step = SmokeStep.ToggleSpecialShown;
	}

	private void VerifySpecialShown()
	{
		if (_settleFrames > 0)
		{
			_settleFrames--;
			return;
		}
		if (!_spellBar.Visible)
			Fail("再次点击仪式按钮后法术栏应显示");
		GD.Print("SMOKE: building action ring ok");
		ReportAndQuit();
	}

	private void Wait(SmokeStep next)
	{
		_settleFrames = SettleFrames;
		_step = next;
	}

	private Button GetButton(int index)
	{
		return _ring.GetChild<Button>(index);
	}

	private void PushBuildingClick(Building building)
	{
		Vector2 screenPoint = GetViewport().GetCanvasTransform() * building.GlobalPosition;
		PushClick(screenPoint);
	}

	private void PushButtonClick(Button button)
	{
		PushClick(button.GetGlobalRect().GetCenter());
	}

	private void PushClick(Vector2 screenPoint)
	{
		if (!_inputScaleReady)
		{
			InputEventMouseButton probe = new()
			{
				ButtonIndex = MouseButton.Left,
				Pressed = true,
				Position = Vector2.One,
				GlobalPosition = Vector2.One,
			};
			GetViewport().PushInput(probe);
		}
		Vector2 inputPosition = screenPoint / _inputScale;
		InputEventMouseMotion motion = new()
		{
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(motion);
		InputEventMouseButton click = new()
		{
			ButtonIndex = MouseButton.Left,
			Pressed = true,
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(click);
		InputEventMouseButton release = new()
		{
			ButtonIndex = MouseButton.Left,
			Pressed = false,
			Position = inputPosition,
			GlobalPosition = screenPoint,
		};
		GetViewport().PushInput(release);
	}

	private Building FindBuilding(string typeId)
	{
		foreach (Building building in _playerCastle.GetBuildings())
		{
			if (building.TypeId == typeId)
				return building;
		}
		return null;
	}

}