// UI 冒烟测试：验证面板装配、Esc 模态优先级与输入屏蔽投影
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class UiSmoke : SmokeTestBase
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";

	private float _elapsed;
	private int _phase;
	private bool _openedSettings;
	private bool _closedSettings;
	private bool _closedPause;
	private bool _reopenedPause;
	private bool _reclosedPause;
	private bool _openedShop;
	private bool _closedShop;

	public override void _Ready()
	{
		StartWatchdog(180f);
		AddChild(GD.Load<PackedScene>(MainGameScene).Instantiate());
	}

	public override void _Process(double delta)
	{
		_elapsed += (float)delta;
		if (_elapsed < 2f)
			return;

		RunGuarded(AdvanceStep);
	}

	private void AdvanceStep()
	{
		if (_phase == 0)
			CheckAssembly();
		else if (_phase == 1)
			OpenModals();
		else if (_phase == 2)
			PressEscape();
		else if (_phase == 3)
			VerifySettingsClosed();
		else if (_phase == 4)
			PressEscape();
		else if (_phase == 5)
			VerifyPauseClosed();
		else if (_phase == 6)
			PressEscape();
		else if (_phase == 7)
			VerifyPauseReopened();
		else if (_phase == 8)
			PressEscape();
		else if (_phase == 9)
			VerifyPauseClosedAgain();
		else if (_phase == 10)
			OpenShop();
		else if (_phase == 11)
			PressEscape();
		else if (_phase == 12)
			VerifyShopClosed();
		else
			ReportAndQuit();

		_phase++;
	}

	private void CheckAssembly()
	{
		RequireNode<Control>(this, "MainGame/UI/Hud");
		RequireNode<Control>(this, "MainGame/UI/Hand");
		RequireNode<Control>(this, "MainGame/UI/SpellBar");
		RequireNode<Control>(this, "MainGame/UI/BuildingInfo");
		RequireNode<Control>(this, "MainGame/UI/UnitInfo");
		RequireNode<Control>(this, "MainGame/UI/Shop");
		RequireNode<Control>(this, "MainGame/UI/PauseMenu");
		RequireNode<Control>(this, "MainGame/UI/GameOver");
		RequireNode<Control>(this, "MainGame/UI/CheatMenu");
		RequireNode<Control>(this, "MainGame/UI/RuntimeInfoPanel");
		RequireNode<Control>(this, "MainGame/UI/ActionRingHost");
		RequireNode<Control>(this, "MainGame/UI/SettingsPanel");
		RequireNode<Control>(this, "MainGame/UI/UnitAttributePanel");
		RequireNode<Control>(this, "MainGame/UI/BuildingAttributePanel");
	}

	private void OpenModals()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		PauseMenuUiController pause = AdapterRegistry.Resolve<PauseMenuUiController>();
		SettingsUiController settings = AdapterRegistry.Resolve<SettingsUiController>();
		if (!pause.Open())
			Fail("暂停菜单无法打开");
		settings.Open();
		if (!pause.IsOpen || !settings.IsOpen)
			Fail("暂停与设置应同时打开");
		if (!gm.IsPaused)
			Fail("暂停菜单打开时游戏应暂停");
		_openedSettings = settings.IsOpen;
	}

	private void PressEscape()
	{
		Input.ParseInputEvent(new InputEventKey
		{
			Keycode = Key.Escape,
			Pressed = true,
		});
	}

	private void VerifySettingsClosed()
	{
		PauseMenuUiController pause = AdapterRegistry.Resolve<PauseMenuUiController>();
		SettingsUiController settings = AdapterRegistry.Resolve<SettingsUiController>();
		if (settings.IsOpen)
			Fail("Esc 应优先关闭设置面板");
		if (!pause.IsOpen)
			Fail("关闭设置后暂停菜单应保持打开");
		_closedSettings = !settings.IsOpen;
	}

	private void VerifyPauseClosed()
	{
		PauseMenuUiController pause = AdapterRegistry.Resolve<PauseMenuUiController>();
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		if (pause.IsOpen)
			Fail("Esc 应关闭暂停菜单");
		if (gm.IsPaused)
			Fail("暂停菜单关闭后游戏应恢复");
		_closedPause = !pause.IsOpen;
	}

	private void VerifyPauseReopened()
	{
		PauseMenuUiController pause = AdapterRegistry.Resolve<PauseMenuUiController>();
		if (!pause.IsOpen)
			Fail("无模态时 Esc 应重新打开暂停菜单");
		_reopenedPause = pause.IsOpen;
	}

	private void VerifyPauseClosedAgain()
	{
		PauseMenuUiController pause = AdapterRegistry.Resolve<PauseMenuUiController>();
		if (pause.IsOpen)
			Fail("Esc 应再次关闭暂停菜单");
		_reclosedPause = !pause.IsOpen;
	}

	private void OpenShop()
	{
		AdapterRegistry.Resolve<Shop>().RequestOpenShop();
		ShopUiController shop = AdapterRegistry.Resolve<ShopUiController>();
		if (!shop.IsOpen)
			Fail("商店应能打开");
		_openedShop = shop.IsOpen;
	}

	private void VerifyShopClosed()
	{
		ShopUiController shop = AdapterRegistry.Resolve<ShopUiController>();
		if (shop.IsOpen)
			Fail("Esc 应关闭商店");
		_closedShop = !shop.IsOpen;
	}
}
