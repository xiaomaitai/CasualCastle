// 开发者模式专属：局内运行时信息面板，按分组显示可选实时数据。
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class RuntimeInfoPanelController
{
	private const string PanelScenePath = "res://scenes/ui/dev_tools/runtime_info_panel.tscn";
	private const string NotStartedText = "对局未开始";

	private readonly Button _openButton;
	private readonly Panel _panel;
	private readonly Button _closeButton;
	private readonly CheckBox _wealthCheck;
	private readonly Label _infoLabel;
	private bool _gameOver;

	public bool IsOpen { get; private set; }

	public RuntimeInfoPanelController(CanvasLayer uiRoot)
	{
		_openButton = uiRoot.GetNode<Button>("PhasePanel/InfoButton");
		_panel = CreatePanel(uiRoot);
		_closeButton = _panel.GetNode<Button>("CloseButton");
		_wealthCheck = _panel.GetNode<CheckBox>("WealthCheck");
		_infoLabel = _panel.GetNode<Label>("InfoLabel");

		_openButton.Pressed += OnOpenPressed;
		_closeButton.Pressed += Close;
		_wealthCheck.Toggled += OnWealthToggled;
		_openButton.Visible = DisplaySettingsManager.DevModeEnabled;
	}

	public void Process()
	{
		if (IsOpen)
			RefreshInfo();
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;
		if (gameOver)
			Close();
	}

	public bool TryHandleEscape()
	{
		if (!IsOpen)
			return false;
		Close();
		return true;
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid(_openButton))
			_openButton.Pressed -= OnOpenPressed;
		if (GodotObject.IsInstanceValid(_closeButton))
			_closeButton.Pressed -= Close;
		if (GodotObject.IsInstanceValid(_wealthCheck))
			_wealthCheck.Toggled -= OnWealthToggled;
	}

	private static Panel CreatePanel(CanvasLayer uiRoot)
	{
		PackedScene scene = GD.Load<PackedScene>(PanelScenePath);
		Panel panel = scene.Instantiate<Panel>();
		uiRoot.AddChild(panel);
		return panel;
	}

	private void OnOpenPressed()
	{
		if (IsOpen)
			Close();
		else
			Open();
	}

	private void Open()
	{
		if (IsOpen || _gameOver)
			return;
		IsOpen = true;
		_panel.Visible = true;
		RefreshInfo();
	}

	private void Close()
	{
		if (!IsOpen)
			return;
		IsOpen = false;
		_panel.Visible = false;
	}

	private void OnWealthToggled(bool enabled)
	{
		RefreshInfo();
	}

	private void RefreshInfo()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		if (gm == null || gm.PlayerCastle == null || gm.EnemyCastle == null)
			_infoLabel.Text = NotStartedText;
		else if (_wealthCheck.ButtonPressed)
			_infoLabel.Text = BuildWealthText(gm);
		else
			_infoLabel.Text = "";
	}

	private string BuildWealthText(GameManager gm)
	{
		int playerPlaced = SumPlacedValue(gm.PlayerCastle);
		int playerHand = SumHandValue();
		int playerGold = GetGold();
		int enemyPlaced = SumPlacedValue(gm.EnemyCastle);
		return string.Join("\n",
			"【财富】",
			$"玩家 财富值：{playerPlaced + playerHand + playerGold}（手牌 {playerHand} / 存款 {playerGold}）",
			$"玩家 已放置财富值：{playerPlaced}",
			$"敌方 财富值：{enemyPlaced}",
			$"敌方 已放置财富值：{enemyPlaced}",
			"敌方无手牌与存款经济");
	}

	private static int SumPlacedValue(Castle castle)
	{
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		int total = 0;
		foreach (Building building in castle.GetBuildings())
		{
			if (building.IsDestroyed)
				continue;
			total += buildingSystem.GetValue(building.TypeId);
		}
		return total;
	}

	private static int SumHandValue()
	{
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		int total = 0;
		foreach (CardData card in AdapterRegistry.Resolve<Hand>().Cards)
			total += buildingSystem.GetValue(card.BuildingType);
		return total;
	}

	private static int GetGold()
	{
		Shop shop = AdapterRegistry.Resolve<Shop>();
		return shop == null ? 0 : shop.Gold;
	}
}