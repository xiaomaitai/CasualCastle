// 开发者模式专属：局内运行时信息面板，按分组显示可选实时数据（runtime_info_panel.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using Godot;
using System.Collections.Generic;

namespace CasualCastle.Adapters.Godot.Dev;

public partial class RuntimeInfoPanelController : Panel
{
	private const string NotStartedText = "对局未开始";

	private Button _openButton;
	private Button _closeButton;
	private CheckBox _wealthCheck;
	private Label _infoLabel;
	private bool _gameOver;

	public bool IsOpen { get; private set; }

	public override void _Ready()
	{
		_openButton = GetParent().GetNode<HudUiController>("Hud").InfoButton;
		_closeButton = GetNode<Button>("CloseButton");
		_wealthCheck = GetNode<CheckBox>("WealthCheck");
		_infoLabel = GetNode<Label>("InfoLabel");

		_openButton.Pressed += OnOpenPressed;
		_closeButton.Pressed += Close;
		_wealthCheck.Toggled += OnWealthToggled;
		_openButton.Visible = DisplaySettingsManager.DevModeEnabled;
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid(_openButton))
			_openButton.Pressed -= OnOpenPressed;
		if (GodotObject.IsInstanceValid(_closeButton))
			_closeButton.Pressed -= Close;
		if (GodotObject.IsInstanceValid(_wealthCheck))
			_wealthCheck.Toggled -= OnWealthToggled;
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
		Visible = true;
		RefreshInfo();
	}

	private void Close()
	{
		if (!IsOpen)
			return;
		IsOpen = false;
		Visible = false;
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
		(int enemyHand, int enemyGold) = GetEnemyEconomy(gm);
		return string.Join("\n",
			"【财富】",
			$"玩家 财富值：{playerPlaced + playerHand + playerGold}（手牌 {playerHand} / 存款 {playerGold}）",
			$"玩家 已放置财富值：{playerPlaced}",
			$"敌方 财富值：{enemyPlaced + enemyHand + enemyGold}（手牌 {enemyHand} / 存款 {enemyGold}）",
			$"敌方 已放置财富值：{enemyPlaced}");
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

	private static (int handValue, int gold) GetEnemyEconomy(GameManager gm)
	{
		BattleReportSystem reportSystem = AdapterRegistry.Resolve<BattleReportSystem>();
		if (reportSystem == null)
			return (0, 0);
		int nightIndex = gm.IsCombat ? gm.CurrentNightIndex + 1 : gm.CurrentNightIndex;
		CastleSnapshot snapshot = reportSystem.GetSelectedPhaseSnapshot(nightIndex, BattlePhase.Day);
		if (snapshot == null)
			return (0, 0);
		return (SumCardValue(snapshot.HandCards), snapshot.Gold);
	}

	private static int SumCardValue(IEnumerable<CardSnapshot> cards)
	{
		BuildingSystem buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
		int total = 0;
		foreach (CardSnapshot card in cards)
			total += buildingSystem.GetValue(card.BuildingType);
		return total;
	}
}
