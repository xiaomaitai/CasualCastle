// 参数编辑器逻辑：把全局参数集中展示、热修改与重置（parameter_editor.tscn 场景）
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Shared;
using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class ParameterEditor
{
	private const string DevToolsMenuScene = "res://scenes/ui/dev_tools/dev_tools_menu.tscn";
	private const string EconomyModule = "经济";
	private const string CombatModule = "战斗";
	private const string DisplayModule = "显示/像素换算";
	private const string AudioModule = "音频";
	private const float NameWidth = 240f;
	private const float EditWidth = 160f;
	private const float ResetWidth = 72f;
	private const float RowHeight = 36f;
	private const float SectionHeight = 32f;
	private const int RowSeparation = 8;
	private const string ConfigDbPath = "res://assets/data/config.db";

	private readonly Control _root;
	private readonly VBoxContainer _list;
	private readonly Button _resetAllButton;
	private readonly Button _backButton;
	private readonly Label _statusBar;
	private readonly List<ParameterRow> _rows = new();
	private readonly SqliteConfigRepository _repo;

	private static readonly ParameterDef[] Defs =
	{
		new ParameterDef("initial_gold", SqliteConfigRepository.EconomyConfig, "每局初始金币", EconomyModule,
			() => GameParameters.InitialGold.ToString(),
			text => TrySetInt(text, value => GameParameters.InitialGold = value),
			GameRules.InitialGold.ToString()),
		new ParameterDef("repair_gold_per_health", SqliteConfigRepository.EconomyConfig, "修理每点生命花费", EconomyModule,
			() => GameParameters.RepairGoldPerHealth.ToString(),
			text => TrySetInt(text, value => GameParameters.RepairGoldPerHealth = value),
			GameRules.RepairGoldPerHealth.ToString()),
		new ParameterDef("combat_duration_seconds", SqliteConfigRepository.BattleConfig, "战斗阶段时长（秒）", CombatModule,
			() => FormatFloat(GameParameters.CombatDurationSeconds),
			text => TrySetFloat(text, value => GameParameters.CombatDurationSeconds = value),
			FormatFloat(GameRules.CombatDurationSeconds)),
		new ParameterDef("combine_duration_seconds", SqliteConfigRepository.BattleConfig, "合成阶段时长（秒）", CombatModule,
			() => FormatFloat(GameParameters.CombineDurationSeconds),
			text => TrySetFloat(text, value => GameParameters.CombineDurationSeconds = value),
			FormatFloat(GameRules.CombineDurationSeconds)),
		new ParameterDef("combat_start_hold_seconds", SqliteConfigRepository.BattleConfig, "战斗开始等待（秒）", CombatModule,
			() => FormatFloat(GameParameters.CombatStartTimerHoldSeconds),
			text => TrySetFloat(text, value => GameParameters.CombatStartTimerHoldSeconds = value),
			FormatFloat(GameRules.CombatStartTimerHoldSeconds)),
		new ParameterDef("pixels_per_cell", SqliteConfigRepository.DisplayConfig, "每格像素（PixelsPerCell）", DisplayModule,
			() => GameCoordinatesAdapter.PixelsPerCell.ToString(),
			text => TrySetInt(text, value => GameCoordinatesAdapter.PixelsPerCell = value),
			GameCoordinatesAdapter.PixelsPerCellDefault.ToString()),
		new ParameterDef("sfx_max_plays_per_second", SqliteConfigRepository.SoundConfig, "音效每秒播放上限", AudioModule,
			() => SfxPolicy.MaxPlaysPerSecond.ToString(),
			text => TrySetInt(text, value => SfxPolicy.MaxPlaysPerSecond = value),
			SfxPolicy.MaxPlaysPerSecondDefault.ToString()),
		new ParameterDef("sfx_group_min_interval_ms", SqliteConfigRepository.SoundConfig, "同类声音最小间隔（毫秒）", AudioModule,
			() => SfxPolicy.GroupMinIntervalMs.ToString(),
			text => TrySetInt(text, value => SfxPolicy.GroupMinIntervalMs = value),
			SfxPolicy.GroupMinIntervalMsDefault.ToString()),
		new ParameterDef("sfx_overload_threshold", SqliteConfigRepository.SoundConfig, "超载重要性阈值", AudioModule,
			() => SfxPolicy.OverloadThreshold.ToString(),
			text => TrySetInt(text, value => SfxPolicy.OverloadThreshold = value),
			SfxPolicy.OverloadThresholdDefault.ToString()),
		new ParameterDef("sfx_threshold_decay_per_second", SqliteConfigRepository.SoundConfig, "阈值回落速度（每1000ms）", AudioModule,
			() => SfxPolicy.ThresholdDecayPerSecond.ToString(),
			text => TrySetInt(text, value => SfxPolicy.ThresholdDecayPerSecond = value),
			SfxPolicy.ThresholdDecayPerSecondDefault.ToString()),
	};

	public ParameterEditor(Control root)
	{
		_root = root;
		_list = root.GetNode<VBoxContainer>("ScrollArea/ScrollContainer/ParamList");
		_resetAllButton = root.GetNode<Button>("BottomBar/ResetAllButton");
		_backButton = root.GetNode<Button>("BottomBar/BackButton");
		_statusBar = root.GetNode<Label>("StatusBar");

		_resetAllButton.Pressed += OnResetAllPressed;
		_backButton.Pressed += OnBackPressed;
		_repo = new SqliteConfigRepository(ProjectSettings.GlobalizePath(ConfigDbPath));
		BuildList();
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid(_resetAllButton))
			_resetAllButton.Pressed -= OnResetAllPressed;
		if (GodotObject.IsInstanceValid(_backButton))
			_backButton.Pressed -= OnBackPressed;
	}

	private void BuildList()
	{
		string currentModule = null;
		foreach (ParameterDef def in Defs)
		{
			if (def.Module != currentModule)
			{
				_list.AddChild(CreateSectionHeader(def.Module));
				currentModule = def.Module;
			}
			_list.AddChild(CreateRow(def));
		}
	}

	private static Control CreateSectionHeader(string module)
	{
		Label header = new()
		{
			Text = "【" + module + "】",
			CustomMinimumSize = new Vector2(0f, SectionHeight),
			VerticalAlignment = VerticalAlignment.Center
		};
		header.AddThemeColorOverride("font_color", new Color(0.95f, 0.8f, 0.4f));
		header.AddThemeFontSizeOverride("font_size", 20);
		return header;
	}

	private Control CreateRow(ParameterDef def)
	{
		HBoxContainer row = new();
		row.AddThemeConstantOverride("separation", RowSeparation);
		row.CustomMinimumSize = new Vector2(0f, RowHeight);

		Label name = new()
		{
			Text = def.Name,
			CustomMinimumSize = new Vector2(NameWidth, 0f),
			VerticalAlignment = VerticalAlignment.Center
		};
		LineEdit edit = new()
		{
			Text = def.Read(),
			CustomMinimumSize = new Vector2(EditWidth, 0f)
		};
		Button reset = new()
		{
			Text = "重置",
			CustomMinimumSize = new Vector2(ResetWidth, 0f)
		};

		row.AddChild(name);
		row.AddChild(edit);
		row.AddChild(reset);

		ParameterRow parameterRow = new(def, edit);
		_rows.Add(parameterRow);
		edit.TextChanged += value => OnTextChanged(parameterRow, value);
		reset.Pressed += () => OnResetPressed(parameterRow);
		return row;
	}

	private void OnTextChanged(ParameterRow row, string text)
	{
		if (row.Def.Apply(text))
		{
			ShowStatus($"已应用 {row.Def.Name} = {row.Edit.Text}");
			WriteToDb(row.Def);
		}
	}

	private void OnResetPressed(ParameterRow row)
	{
		row.Def.Apply(row.Def.Default);
		row.Edit.Text = row.Def.Default;
		ShowStatus($"已重置 {row.Def.Name} = {row.Def.Default}");
		WriteToDb(row.Def);
	}

	private void OnResetAllPressed()
	{
		foreach (ParameterRow row in _rows)
		{
			row.Def.Apply(row.Def.Default);
			row.Edit.Text = row.Def.Default;
			WriteToDb(row.Def);
		}
		ShowStatus("已全部重置为默认值");
	}

	private void OnBackPressed()
	{
		_root.GetTree().ChangeSceneToFile(DevToolsMenuScene);
	}

	private void ShowStatus(string message)
	{
		_statusBar.Text = message;
	}

	private void WriteToDb(ParameterDef def)
	{
		if (double.TryParse(def.Read(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
			_repo.Write(def.Table, def.Key, value);
	}

	private static bool TrySetInt(string text, Action<int> setter)
	{
		if (!int.TryParse(text, out int value))
			return false;
		setter(value);
		return true;
	}

	private static bool TrySetFloat(string text, Action<float> setter)
	{
		if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
			return false;
		setter(value);
		return true;
	}

	private static string FormatFloat(float value) =>
		value.ToString(CultureInfo.InvariantCulture);

	private sealed class ParameterDef
	{
		public readonly string Key;
		public readonly string Table;
		public readonly string Name;
		public readonly string Module;
		public readonly Func<string> Read;
		public readonly Func<string, bool> Apply;
		public readonly string Default;

		public ParameterDef(string key, string table, string name, string module, Func<string> read, Func<string, bool> apply, string defaultValue)
		{
			Key = key;
			Table = table;
			Name = name;
			Module = module;
			Read = read;
			Apply = apply;
			Default = defaultValue;
		}
	}

	private sealed class ParameterRow
	{
		public readonly ParameterDef Def;
		public readonly LineEdit Edit;

		public ParameterRow(ParameterDef def, LineEdit edit)
		{
			Def = def;
			Edit = edit;
		}
	}
}
