// 伤害矩阵编辑器逻辑
using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Godot;
using Microsoft.Data.Sqlite;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class DamageMatrixEditor
{
	private const string DevToolsMenuScene = "res://scenes/ui/dev_tools/dev_tools_menu.tscn";
	private const string TitleScene = "res://scenes/ui/title_screen.tscn";
	private const int HeaderHeight = 36;
	private const int CellWidth = 80;
	private const int CellHeight = 34;
	private const int RowLabelWidth = 56;
	private const int ArmorCount = 7;
	private const int DamageCount = 7;
	private const float GridWidth = RowLabelWidth + ArmorCount * CellWidth;
	private const float GridHeight = HeaderHeight + DamageCount * CellHeight;

	private static readonly string[] ArmorLabels = { "无甲", "布甲", "皮甲", "锁甲", "板甲", "城防", "神圣" };
	private static readonly string[] DamageLabels = { "挥砍", "穿刺", "钝击", "攻城", "魔法", "神圣", "混沌" };

	private static readonly DamageType[] DamageTypes =
	{
		DamageType.Slash, DamageType.Pierce, DamageType.Blunt,
		DamageType.Siege, DamageType.Magic, DamageType.Holy, DamageType.Chaos
	};

	private static readonly ArmorType[] ArmorTypes =
	{
		ArmorType.Unarmored, ArmorType.Cloth, ArmorType.Leather,
		ArmorType.Chainmail, ArmorType.Plate, ArmorType.Fortification, ArmorType.Holy
	};

	private readonly Control _root;
	private readonly Button _saveButton;
	private readonly Button _backButton;
	private readonly Control _gridContainer;
	private readonly Dictionary<(int, int), LineEdit> _cells = new();

	public DamageMatrixEditor(Control root)
	{
		_root = root;
		_saveButton = root.GetNode<Button>("BottomBar/SaveButton");
		_backButton = root.GetNode<Button>("BottomBar/BackButton");
		_gridContainer = root.GetNode<Control>("GridArea/GridPanel");

		_saveButton.Pressed += OnSavePressed;
		_backButton.Pressed += OnBackPressed;
		_gridContainer.Draw += OnGridDraw;

		BuildGrid();
		LoadFromDatabase();
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid(_saveButton))
			_saveButton.Pressed -= OnSavePressed;
		if (GodotObject.IsInstanceValid(_backButton))
			_backButton.Pressed -= OnBackPressed;
		if (GodotObject.IsInstanceValid(_gridContainer))
			_gridContainer.Draw -= OnGridDraw;
	}

	private void BuildGrid()
	{
		for (int col = 0; col < ArmorCount; col++)
		{
			Label header = new()
			{
				Text = ArmorLabels[col],
				Position = new Vector2(RowLabelWidth + col * CellWidth, 0),
				Size = new Vector2(CellWidth, HeaderHeight),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			header.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
			_gridContainer.AddChild(header);
		}

		for (int row = 0; row < DamageCount; row++)
		{
			Label rowLabel = new()
			{
				Text = DamageLabels[row],
				Position = new Vector2(0, HeaderHeight + row * CellHeight),
				Size = new Vector2(RowLabelWidth, CellHeight),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			rowLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
			_gridContainer.AddChild(rowLabel);

			for (int col = 0; col < ArmorCount; col++)
			{
				LineEdit cell = new()
				{
					Position = new Vector2(RowLabelWidth + col * CellWidth + 2, HeaderHeight + row * CellHeight + 2),
					Size = new Vector2(CellWidth - 4, CellHeight - 4),
					Alignment = HorizontalAlignment.Center,
					Editable = true
				};
				StyleBoxFlat cellStyle = new()
				{
					BgColor = new Color(0.18f, 0.18f, 0.22f),
					BorderWidthBottom = 0,
					BorderWidthTop = 0,
					BorderWidthLeft = 0,
					BorderWidthRight = 0
				};
				cell.AddThemeStyleboxOverride("normal", cellStyle);
				cell.AddThemeStyleboxOverride("focus", cellStyle);
				_gridContainer.AddChild(cell);
				_cells[(row, col)] = cell;
			}
		}
	}

	private void OnGridDraw()
	{
		Color lineColor = new(0.5f, 0.5f, 0.55f);

		for (int row = 0; row <= DamageCount; row++)
		{
			float y = HeaderHeight + row * CellHeight;
			_gridContainer.DrawLine(new Vector2(0, y), new Vector2(GridWidth, y), lineColor);
		}
		for (int col = 0; col <= ArmorCount; col++)
		{
			float x = RowLabelWidth + col * CellWidth;
			_gridContainer.DrawLine(new Vector2(x, 0), new Vector2(x, GridHeight), lineColor);
		}
	}

	private void LoadFromDatabase()
	{
		DamageMatrix dm = GameManager.Get<DamageMatrix>();

		for (int row = 0; row < DamageTypes.Length; row++)
		{
			for (int col = 0; col < ArmorTypes.Length; col++)
			{
				float value = dm.GetMultiplier(DamageTypes[row], ArmorTypes[col]);
				if (_cells.TryGetValue((row, col), out LineEdit cell))
					cell.Text = value.ToString("F1");
			}
		}
	}

	private void OnSavePressed()
	{
		string dbPath = ProjectSettings.GlobalizePath("res://assets/data/config.db");
		using SqliteConnection connection = new($"Data Source={dbPath}");
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();

		cmd.CommandText = "DELETE FROM damage_matrix";
		cmd.ExecuteNonQuery();

		for (int row = 0; row < DamageTypes.Length; row++)
		{
			for (int col = 0; col < ArmorTypes.Length; col++)
			{
				string text = _cells[(row, col)].Text;
				float value = ParseFloat(text);

				cmd.CommandText = "INSERT INTO damage_matrix (damage_type, armor_type, multiplier) VALUES (@d, @a, @m)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@d", (int)DamageTypes[row]);
				cmd.Parameters.AddWithValue("@a", (int)ArmorTypes[col]);
				cmd.Parameters.AddWithValue("@m", value);
				cmd.ExecuteNonQuery();
			}
		}

		DamageMatrix dm = GameManager.Get<DamageMatrix>();
		for (int row = 0; row < DamageTypes.Length; row++)
		{
			for (int col = 0; col < ArmorTypes.Length; col++)
			{
				float value = ParseFloat(_cells[(row, col)].Text);
				dm.Set(DamageTypes[row], ArmorTypes[col], value);
			}
		}

		ShowStatus("保存成功");
	}

	private void OnBackPressed()
	{
		_root.GetTree().ChangeSceneToFile(DevToolsMenuScene);
	}

	private static float ParseFloat(string text)
	{
		if (float.TryParse(text, out float result))
			return result;
		return 1.0f;
	}

	private async void ShowStatus(string message)
	{
		Label statusBar = _root.GetNode<Label>("StatusBar");
		statusBar.Text = message;
		await _root.ToSignal(_root.GetTree().CreateTimer(2), "timeout");
		statusBar.Text = "就绪";
	}
}
