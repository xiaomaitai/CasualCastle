using System;
using System.Collections.Generic;
using System.Linq;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Building;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public class TechTreeEditorController
{
	private readonly Control _root;
	private readonly Button _prevRaceButton;
	private readonly Label _raceNameLabel;
	private readonly Button _nextRaceButton;
	private readonly Button _saveButton;
	private readonly Button _closeButton;
	private readonly Control _cardCanvas;
	private readonly Control _libraryPanel;
	private readonly VBoxContainer _libraryList;
	private readonly Button _newBuildingButton;
	private readonly Label _statusBar;

	private readonly ITechTreeRepository _repo;
	private List<RaceDef> _races = new();
	private List<TechTreeNode> _nodes = new();
	private List<CombineRecipe> _edges = new();
	private string _currentRaceId = "";
	private int _currentRaceIndex = 0;

	private enum DragMode { None, MoveCard, CreateConnection, LibraryToCanvas, CanvasToLibrary }
	private DragMode _dragMode;
	private Panel _dragSource;
	private Panel _dragPreview;
	private Vector2 _dragOffset;
	private bool _dragActive;

	private const float CardWidth = 120;
	private const float CardHeight = 56;
	private const float ColSpacing = 140;

	public TechTreeEditorController(Control root)
	{
		_root = root;
		_prevRaceButton = root.GetNode<Button>("TopBar/PrevRaceButton");
		_raceNameLabel = root.GetNode<Label>("TopBar/RaceNameLabel");
		_nextRaceButton = root.GetNode<Button>("TopBar/NextRaceButton");
		_saveButton = root.GetNode<Button>("TopBar/SaveButton");
		_closeButton = root.GetNode<Button>("TopBar/CloseButton");
		_cardCanvas = root.GetNode<Control>("EditorArea/CardCanvas");
		_libraryPanel = root.GetNode<Control>("EditorArea/LibraryPanel");
		_libraryList = root.GetNode<VBoxContainer>("EditorArea/LibraryPanel/LibraryScroll/LibraryList");
		_newBuildingButton = root.GetNode<Button>("EditorArea/LibraryPanel/NewBuildingButton");
		_statusBar = root.GetNode<Label>("StatusBar");

		_repo = new SqliteTechTreeRepository();

		_prevRaceButton.Pressed += OnPrevRacePressed;
		_nextRaceButton.Pressed += OnNextRacePressed;
		_saveButton.Pressed += OnSavePressed;
		_closeButton.Pressed += OnClosePressed;
		_newBuildingButton.Pressed += OnNewBuildingPressed;
		_cardCanvas.Draw += OnCardCanvasDraw;
		_cardCanvas.GuiInput += OnCardCanvasGuiInput;
	}

	public void LoadInitialData()
	{
		_races = _repo.LoadRaces();
		if (_races.Count == 0)
			return;
		_currentRaceIndex = 0;
		LoadRaceAtIndex(0);
	}

	public void Dispose()
	{
		_prevRaceButton.Pressed -= OnPrevRacePressed;
		_nextRaceButton.Pressed -= OnNextRacePressed;
		_saveButton.Pressed -= OnSavePressed;
		_closeButton.Pressed -= OnClosePressed;
		_newBuildingButton.Pressed -= OnNewBuildingPressed;
		_cardCanvas.Draw -= OnCardCanvasDraw;
		_cardCanvas.GuiInput -= OnCardCanvasGuiInput;
	}

	public void Process()
	{
		if (!_dragActive || _dragSource == null || !GodotObject.IsInstanceValid(_dragSource))
		{
			if (_dragActive && (_dragSource == null || !GodotObject.IsInstanceValid(_dragSource)))
				CancelDrag();
			return;
		}

		if (_dragMode == DragMode.MoveCard)
		{
			if (_dragPreview != null && GodotObject.IsInstanceValid(_dragPreview))
			{
				Vector2 localPos = _cardCanvas.GetLocalMousePosition();
				_dragPreview.Position = localPos - _dragOffset;
			}
		}
		else if (_dragMode == DragMode.CreateConnection)
		{
			_cardCanvas.QueueRedraw();
		}
		else if (_dragMode == DragMode.LibraryToCanvas)
		{
			if (_dragPreview != null && GodotObject.IsInstanceValid(_dragPreview))
			{
				_dragPreview.GlobalPosition = _root.GetGlobalMousePosition() - _dragOffset;
			}
		}
	}

	public void HandleGlobalInput(InputEvent @event)
	{
		if (!_dragActive)
			return;

		if (@event is not InputEventMouseButton mouseUp || mouseUp.Pressed)
			return;

		if (_dragSource == null || !GodotObject.IsInstanceValid(_dragSource))
		{
			CancelDrag();
			return;
		}

		if (_dragMode == DragMode.MoveCard)
		{
			if (mouseUp.ButtonIndex == MouseButton.Left)
			{
				if (IsMouseOverLibrary())
					CanvasToLibrary(_dragSource);
				else
					EndMoveDrag();
				return;
			}
		}

		if (_dragMode == DragMode.CreateConnection && mouseUp.ButtonIndex == MouseButton.Right)
		{
			EndConnectionDrag();
			return;
		}

		if (_dragMode == DragMode.LibraryToCanvas && mouseUp.ButtonIndex == MouseButton.Left)
		{
			EndLibraryToCanvasDrag();
			return;
		}

		if (mouseUp.ButtonIndex == MouseButton.Left || mouseUp.ButtonIndex == MouseButton.Right)
			CancelDrag();
	}

	private void OnPrevRacePressed()
	{
		if (_currentRaceIndex > 0)
		{
			_currentRaceIndex--;
			GD.Print($"[TechTreeEditor] 切换到上一个种族, 索引={_currentRaceIndex}, ID={_races[_currentRaceIndex].Id}");
			LoadRaceAtIndex(_currentRaceIndex);
		}
	}

	private void OnNextRacePressed()
	{
		if (_currentRaceIndex < _races.Count - 1)
		{
			_currentRaceIndex++;
			GD.Print($"[TechTreeEditor] 切换到下一个种族, 索引={_currentRaceIndex}, ID={_races[_currentRaceIndex].Id}");
			LoadRaceAtIndex(_currentRaceIndex);
		}
	}

	private void LoadRaceAtIndex(int index)
	{
		GD.Print($"[TechTreeEditor] LoadRaceAtIndex index={index}, raceId={_races[index].Id}, 切换前_nodes.Count={_nodes.Count}");
		_currentRaceIndex = index;
		RaceDef race = _races[index];
		_currentRaceId = race.Id;
		_raceNameLabel.Text = race.DisplayName;
		_nodes = _repo.LoadNodes(race.Id);
		_edges = _repo.LoadEdges(race.Id);
		GD.Print($"[TechTreeEditor] 加载完成: raceId={race.Id}, nodes={_nodes.Count}, edges={_edges.Count}");
		LayoutCards();
		RefreshLibrary();
		ShowStatus($"已加载 {_nodes.Count} 个节点，{_edges.Count} 条连线");
	}

	private void LayoutCards()
	{
		foreach (Node child in _cardCanvas.GetChildren())
		{
			if (child is Panel panel && panel.GetMeta("type_id").AsString() != null)
				child.QueueFree();
		}

		if (_nodes.Count == 0)
		{
			_cardCanvas.QueueRedraw();
			return;
		}

		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;

		foreach (TechTreeNode node in _nodes)
		{
			Panel card = CreateCardView(node);
			float x = node.Col * ColSpacing + 10;
			float y = (4 - (node.Tier - 1)) * rowHeight + (rowHeight - CardHeight) / 2;
			card.Position = new Vector2(x, y);
			_cardCanvas.AddChild(card);
		}

		_cardCanvas.QueueRedraw();
	}

	private Panel CreateCardView(TechTreeNode node)
	{
		Panel panel = new()
		{
			CustomMinimumSize = new Vector2(CardWidth, CardHeight),
			Size = new Vector2(CardWidth, CardHeight),
			MouseFilter = Control.MouseFilterEnum.Stop
		};

		StyleBoxFlat styleBox = new();
		bool hasIncomingEdge = _edges.Exists(e => e.ResultTypeId == node.TypeId);
		styleBox.BgColor = hasIncomingEdge
			? new Color(0.15f, 0.25f, 0.5f)
			: new Color(0.15f, 0.5f, 0.15f);
		panel.AddThemeStyleboxOverride("panel", styleBox);

		Label nameLabel = new()
		{
			Text = node.DisplayName,
			Position = new Vector2(4, 0),
			Size = new Vector2(CardWidth - 8, CardHeight),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		panel.AddChild(nameLabel);

		panel.SetMeta("type_id", node.TypeId);
		panel.SetMeta("tier", node.Tier);

		panel.GuiInput += e => OnCardGuiInput(panel, e);

		return panel;
	}

	private void RefreshLibrary()
	{
		for (int i = _libraryList.GetChildCount() - 1; i >= 0; i--)
		{
			Node child = _libraryList.GetChild(i);
			_libraryList.RemoveChild(child);
			child.QueueFree();
		}

		List<BuildingTypeSummary> allTypes = _repo.LoadAllBuildingTypes(_currentRaceId);
		int placedCount = 0;
		GD.Print($"[TechTreeEditor] RefreshLibrary: 建筑总数={allTypes.Count}, 当前种族节点数={_nodes.Count}");

		foreach (BuildingTypeSummary type in allTypes)
		{
			bool placed = _nodes.Exists(n => n.TypeId == type.TypeId);
			if (placed) placedCount++;
			Panel item = CreateLibraryItem(type.TypeId, type.DisplayName, placed);
			_libraryList.AddChild(item);
		}
		GD.Print($"[TechTreeEditor] RefreshLibrary 完成: 已放置={placedCount}, 未放置={allTypes.Count - placedCount}");
	}

	private Panel CreateLibraryItem(string typeId, string displayName, bool placed)
	{
		Panel panel = new()
		{
			CustomMinimumSize = new Vector2(180, 36),
			Size = new Vector2(180, 36),
			MouseFilter = placed ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop
		};

		StyleBoxFlat styleBox = new();
		styleBox.BgColor = placed
			? new Color(0.25f, 0.25f, 0.25f)
			: new Color(0.2f, 0.4f, 0.55f);
		panel.AddThemeStyleboxOverride("panel", styleBox);

		Color labelColor = placed
			? new Color(0.5f, 0.5f, 0.5f)
			: new Color(1f, 1f, 1f);

		Label label = new()
		{
			Text = displayName,
			Position = new Vector2(4, 8),
			Size = new Vector2(172, 20),
			HorizontalAlignment = HorizontalAlignment.Center,
			Modulate = labelColor
		};
		panel.AddChild(label);

		panel.SetMeta("type_id", typeId);
		panel.SetMeta("display_name", displayName);

		if (!placed)
			panel.GuiInput += e => OnLibraryItemGuiInput(panel, e);

		return panel;
	}

	private void OnCardGuiInput(Panel card, InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (mouseButton.DoubleClick)
				{
					OnCardDoubleClick(card);
					return;
				}

				_dragMode = DragMode.MoveCard;
				_dragSource = card;
				_dragActive = true;
				_dragOffset = _cardCanvas.GetLocalMousePosition() - card.Position;
				CreateDragPreview(card);
				return;
			}

			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				_dragMode = DragMode.CreateConnection;
				_dragSource = card;
				_dragActive = true;
				return;
			}
		}
	}

	private void OnLibraryItemGuiInput(Panel item, InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			_dragMode = DragMode.LibraryToCanvas;
			_dragSource = item;
			_dragActive = true;
			_dragOffset = _root.GetGlobalMousePosition() - item.GlobalPosition;
			CreateDragPreview(item);
			item.AcceptEvent();
		}
	}

	private void OnCardCanvasGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton rightClick && rightClick.Pressed && rightClick.ButtonIndex == MouseButton.Right)
		{
			if (!_dragActive)
			{
				Vector2 clickPos = _cardCanvas.GetLocalMousePosition();
				TryDeleteEdgeAt(clickPos);
			}
		}
	}

	private void CreateDragPreview(Panel source)
	{
		_dragPreview = new Panel
		{
			CustomMinimumSize = source.Size,
			Size = source.Size,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Modulate = new Color(1, 1, 1, 0.5f)
		};

		StyleBoxFlat styleBox = new();
		styleBox.BgColor = source.GetThemeStylebox("panel") is StyleBoxFlat sbf
			? sbf.BgColor
			: new Color(0.5f, 0.5f, 0.5f);
		_dragPreview.AddThemeStyleboxOverride("panel", styleBox);

		_root.AddChild(_dragPreview);
		_dragPreview.GlobalPosition = source.GlobalPosition;
	}

	private void EndMoveDrag()
	{
		if (_dragSource == null)
		{
			CancelDrag();
			return;
		}

		Vector2 localPos = _cardCanvas.GetLocalMousePosition();
		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;
		int newTier = Math.Clamp(5 - (int)(localPos.Y / rowHeight), 1, 5);
		int newCol = Math.Max(0, (int)Math.Round((localPos.X - _dragOffset.X - 10) / ColSpacing));

		TechTreeNode node = _nodes.Find(n => n.TypeId == _dragSource.GetMeta("type_id").AsString());
		if (node != null)
		{
			int oldTier = node.Tier;
			node.Tier = newTier;
			node.Col = newCol;

			if (newTier != oldTier)
				RemoveInvalidEdges(node);
		}

		LayoutCards();
		CancelDrag();
	}

	private void RemoveInvalidEdges(TechTreeNode movedNode)
	{
		List<CombineRecipe> invalidEdges = _edges.Where(e =>
		{
			if (e.MainTypeId == movedNode.TypeId)
			{
				TechTreeNode resultNode = _nodes.Find(n => n.TypeId == e.ResultTypeId);
				return resultNode != null && resultNode.Tier <= movedNode.Tier;
			}
			if (e.ResultTypeId == movedNode.TypeId)
			{
				TechTreeNode sourceNode = _nodes.Find(n => n.TypeId == e.MainTypeId);
				return sourceNode != null && movedNode.Tier <= sourceNode.Tier;
			}
			return false;
		}).ToList();

		foreach (CombineRecipe edge in invalidEdges)
			_repo.RemoveRecipe(edge.MainTypeId, edge.MaterialTypeId, edge.ResultTypeId);

		if (invalidEdges.Count > 0)
		{
			_edges = _repo.LoadEdges(_currentRaceId);
			ShowStatus($"已移除 {invalidEdges.Count} 条无效连线");
		}
	}

	private void EndConnectionDrag()
	{
		Vector2 localPos = _cardCanvas.GetLocalMousePosition();
		Panel targetCard = FindCardAtPosition(localPos);

		if (targetCard != null && targetCard != _dragSource)
		{
			string dragId = _dragSource.GetMeta("type_id").AsString();
			string dropId = targetCard.GetMeta("type_id").AsString();

			TechTreeNode dragNode = _nodes.Find(n => n.TypeId == dragId);
			TechTreeNode dropNode = _nodes.Find(n => n.TypeId == dropId);

			if (dragNode == null || dropNode == null)
			{
				CancelDrag();
				return;
			}

			if (dragNode.Tier == dropNode.Tier)
			{
				ShowStatus("同层级建筑不可连线");
				CancelDrag();
				return;
			}

			bool dragIsLower = dragNode.Tier < dropNode.Tier;
			string fromId = dragIsLower ? dragId : dropId;
			string toId = dragIsLower ? dropId : dragId;

			CombineRecipe recipe = new()
			{
				MainTypeId = fromId,
				MaterialTypeId = fromId,
				MaterialCount = 2,
				ResultTypeId = toId
			};
			_repo.AddRecipe(recipe);
			_edges = _repo.LoadEdges(_currentRaceId);
			LayoutCards();
			ShowStatus($"连线已创建: {fromId} → {toId}");
		}

		CancelDrag();
	}

	private void EndLibraryToCanvasDrag()
	{
		Vector2 localPos = _cardCanvas.GetLocalMousePosition();

		if (!IsPointInCanvas(localPos))
		{
			CancelDrag();
			return;
		}

		string typeId = _dragSource.GetMeta("type_id").AsString();
		if (_nodes.Exists(n => n.TypeId == typeId))
		{
			CancelDrag();
			return;
		}

		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;
		int tier = Math.Clamp(5 - (int)(localPos.Y / rowHeight), 1, 5);

		int maxCol = -1;
		foreach (TechTreeNode n in _nodes.Where(n => n.Tier == tier))
		{
			if (n.Col > maxCol)
				maxCol = n.Col;
		}

		List<BuildingTypeSummary> allTypes = _repo.LoadAllBuildingTypes(_currentRaceId);
		BuildingTypeSummary typeInfo = allTypes.Find(t => t.TypeId == typeId);
		string displayName = typeInfo?.DisplayName ?? typeId;

		TechTreeNode newNode = new()
		{
			TypeId = typeId,
			RaceId = _currentRaceId,
			Tier = tier,
			Col = maxCol + 1,
			ShopAvailable = false,
			DisplayName = displayName,
			UnitTypeId = typeId,
			MaxHealth = 200,
			SpawnInterval = 10.0f
		};

		_nodes.Add(newNode);
		LayoutCards();
		RefreshLibrary();
		CancelDrag();
		ShowStatus($"已添加: {displayName} (T{tier})");
	}

	private void CanvasToLibrary(Panel card)
	{
		string typeId = card.GetMeta("type_id").AsString();

		List<CombineRecipe> affectedRecipes = _edges.Where(e =>
			e.MainTypeId == typeId || e.MaterialTypeId == typeId || e.ResultTypeId == typeId).ToList();

		foreach (CombineRecipe recipe in affectedRecipes)
		{
			if (recipe.ResultTypeId == typeId)
			{
				_repo.RemoveRecipe(recipe.MainTypeId, recipe.MaterialTypeId, recipe.ResultTypeId);
				continue;
			}

			string otherBuilding = recipe.MainTypeId == typeId ? recipe.MaterialTypeId : recipe.MainTypeId;
			bool otherIsSelf = recipe.MainTypeId == recipe.MaterialTypeId;

			if (otherIsSelf || otherBuilding == typeId)
			{
				_repo.RemoveRecipe(recipe.MainTypeId, recipe.MaterialTypeId, recipe.ResultTypeId);
				continue;
			}

			_repo.RemoveRecipe(recipe.MainTypeId, recipe.MaterialTypeId, recipe.ResultTypeId);

			CombineRecipe newRecipe = new()
			{
				MainTypeId = otherBuilding,
				MaterialTypeId = otherBuilding,
				MaterialCount = 2,
				ResultTypeId = recipe.ResultTypeId
			};
			_repo.AddRecipe(newRecipe);
		}

		_nodes.RemoveAll(n => n.TypeId == typeId);
		_edges = _repo.LoadEdges(_currentRaceId);
		LayoutCards();
		RefreshLibrary();
		ShowStatus($"已移除: {typeId}");
	}

	private void TryDeleteEdgeAt(Vector2 canvasPos)
	{
		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;
		float tolerance = 10f;

		CombineRecipe toDelete = null;
		foreach (CombineRecipe edge in _edges)
		{
			Vector2 fromCenter = GetCardBottomCenter(edge.MainTypeId, rowHeight);
			Vector2 toCenter = GetCardTopCenter(edge.ResultTypeId, rowHeight);
			if (fromCenter == Vector2.Zero || toCenter == Vector2.Zero)
				continue;

			float dist = DistanceToBezier(canvasPos, fromCenter, toCenter);
			if (dist < tolerance)
			{
				toDelete = edge;
				tolerance = dist;
			}
		}

		if (toDelete != null)
		{
			_repo.RemoveRecipe(toDelete.MainTypeId, toDelete.MaterialTypeId, toDelete.ResultTypeId);
			_edges = _repo.LoadEdges(_currentRaceId);
			LayoutCards();
			ShowStatus($"连线已删除: {toDelete.MainTypeId} → {toDelete.ResultTypeId}");
		}
	}

	private void OnCardDoubleClick(Panel card)
	{
		string typeId = card.GetMeta("type_id").AsString();
		TechTreeNode node = _nodes.Find(n => n.TypeId == typeId);
		if (node == null)
			return;

		AcceptDialog dialog = new()
		{
			Title = $"编辑: {node.DisplayName}",
			Size = new Vector2I(300, 280)
		};

		VBoxContainer content = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

		HBoxContainer nameRow = CreateEditRow("名称", out LineEdit nameEdit);
		nameEdit.Text = node.DisplayName;
		content.AddChild(nameRow);

		HBoxContainer costRow = CreateEditRow("费用", out SpinBox costSpin);
		costSpin.MinValue = 0;
		costSpin.MaxValue = 99;
		costSpin.Value = node.GoldCost;
		content.AddChild(costRow);

		HBoxContainer weightRow = CreateEditRow("权重", out SpinBox weightSpin);
		weightSpin.MinValue = 0;
		weightSpin.MaxValue = 99;
		weightSpin.Value = node.ShopWeight;
		content.AddChild(weightRow);

		HBoxContainer tierRow = CreateEditRow("层级", out SpinBox tierSpin);
		tierSpin.MinValue = 1;
		tierSpin.MaxValue = 5;
		tierSpin.Value = node.Tier;
		content.AddChild(tierRow);

		CheckBox shopCheck = new() { Text = "商店可买", ButtonPressed = node.ShopAvailable };
		content.AddChild(shopCheck);

		dialog.AddChild(content);
		_root.AddChild(dialog);

		dialog.Confirmed += () =>
		{
			int oldTier = node.Tier;
			node.DisplayName = nameEdit.Text;
			node.GoldCost = (int)costSpin.Value;
			node.ShopWeight = (int)weightSpin.Value;
			node.Tier = (int)tierSpin.Value;
			node.ShopAvailable = shopCheck.ButtonPressed;
			if (node.Tier != oldTier)
				RemoveInvalidEdges(node);
			LayoutCards();
			RefreshLibrary();
			dialog.QueueFree();
		};

		dialog.Canceled += () => dialog.QueueFree();
		dialog.PopupCentered();
	}

	private void OnNewBuildingPressed()
	{
		AcceptDialog dialog = new()
		{
			Title = "新建建筑",
			Size = new Vector2I(300, 180)
		};

		VBoxContainer content = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

		HBoxContainer idRow = CreateEditRow("TypeID", out LineEdit idEdit);
		idEdit.Text = "NewBuilding";
		content.AddChild(idRow);

		HBoxContainer nameRow = CreateEditRow("名称", out LineEdit nameEdit);
		nameEdit.Text = "新建筑";
		content.AddChild(nameRow);

		dialog.AddChild(content);
		_root.AddChild(dialog);

		dialog.Confirmed += () =>
		{
			string typeId = idEdit.Text.Trim();
			string displayName = nameEdit.Text.Trim();
			if (string.IsNullOrEmpty(typeId) || string.IsNullOrEmpty(displayName))
			{
				dialog.QueueFree();
				return;
			}

			string dbPath = global::Godot.ProjectSettings.GlobalizePath("res://assets/data/config.db");
			using Microsoft.Data.Sqlite.SqliteConnection conn = new($"Data Source={dbPath}");
			conn.Open();
			using Microsoft.Data.Sqlite.SqliteCommand insertCmd = conn.CreateCommand();
			insertCmd.CommandText = "INSERT OR IGNORE INTO building_defs (type_id, display_name, race_id, max_health, spawn_interval, footprint_json, collision_width, collision_height) VALUES (@id, @name, @raceId, 200, 10.0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188)";
			insertCmd.Parameters.AddWithValue("@id", typeId);
			insertCmd.Parameters.AddWithValue("@name", displayName);
			insertCmd.Parameters.AddWithValue("@raceId", _currentRaceId);
			insertCmd.ExecuteNonQuery();

			RefreshLibrary();
			ShowStatus($"新建建筑: {displayName}");
			dialog.QueueFree();
		};

		dialog.Canceled += () => dialog.QueueFree();
		dialog.PopupCentered();
	}

	private static HBoxContainer CreateEditRow(string labelText, out LineEdit lineEdit)
	{
		HBoxContainer row = new();
		Label label = new() { Text = labelText, CustomMinimumSize = new Vector2(60, 0) };
		row.AddChild(label);
		lineEdit = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		row.AddChild(lineEdit);
		return row;
	}

	private static HBoxContainer CreateEditRow(string labelText, out SpinBox spinBox)
	{
		HBoxContainer row = new();
		Label label = new() { Text = labelText, CustomMinimumSize = new Vector2(60, 0) };
		row.AddChild(label);
		spinBox = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		row.AddChild(spinBox);
		return row;
	}

	private Panel FindCardAtPosition(Vector2 canvasPos)
	{
		foreach (Node child in _cardCanvas.GetChildren())
		{
			if (child is Panel panel && panel.GetMeta("type_id").AsString() != null)
			{
				Rect2 bounds = new(panel.Position, panel.Size);
				if (bounds.HasPoint(canvasPos))
					return panel;
			}
		}
		return null;
	}

	private bool IsPointInCanvas(Vector2 canvasPos)
	{
		return canvasPos.X >= 0 && canvasPos.Y >= 0
			&& canvasPos.X <= _cardCanvas.Size.X && canvasPos.Y <= _cardCanvas.Size.Y;
	}

	private bool IsMouseOverLibrary()
	{
		Vector2 libLocal = _libraryPanel.GetLocalMousePosition();
		return libLocal.X >= 0 && libLocal.Y >= 0
			&& libLocal.X <= _libraryPanel.Size.X && libLocal.Y <= _libraryPanel.Size.Y;
	}

	private void CancelDrag()
	{
		_dragMode = DragMode.None;
		_dragSource = null;
		_dragActive = false;
		_dragPreview?.QueueFree();
		_dragPreview = null;
		_cardCanvas.QueueRedraw();
	}

	private static int GetNodeCol(Panel card)
	{
		return (int)Math.Round((card.Position.X - 10) / ColSpacing);
	}

	private float DistanceToBezier(Vector2 point, Vector2 from, Vector2 to)
	{
		Vector2 c1 = from + new Vector2(0, 30);
		Vector2 c2 = to - new Vector2(0, 30);
		float minDist = float.MaxValue;
		for (int i = 0; i < 40; i++)
		{
			float t = i / 39f;
			Vector2 p = CubicBezier(from, c1, c2, to, t);
			float dist = point.DistanceTo(p);
			if (dist < minDist)
				minDist = dist;
		}
		return minDist;
	}

	private Vector2 GetCardCenter(string typeId, float rowHeight, float yOffset)
	{
		TechTreeNode node = _nodes.Find(n => n.TypeId == typeId);
		if (node == null)
			return Vector2.Zero;

		float x = node.Col * ColSpacing + 10 + CardWidth / 2;
		float y = (4 - (node.Tier - 1)) * rowHeight + rowHeight / 2 + yOffset;
		return new Vector2(x, y);
	}

	private Vector2 GetCardBottomCenter(string typeId, float rowHeight)
	{
		return GetCardCenter(typeId, rowHeight, CardHeight / 2);
	}

	private Vector2 GetCardTopCenter(string typeId, float rowHeight)
	{
		return GetCardCenter(typeId, rowHeight, -CardHeight / 2);
	}

	private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		float u = 1 - t;
		return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
	}

	private void OnSavePressed()
	{
		_repo.SaveNodes(_currentRaceId, _nodes);
		_repo.SyncToGameTables(_currentRaceId);
		ShowStatus("保存成功");
	}

	private void OnClosePressed()
	{
		_root.GetTree().Quit();
	}

	private void OnCardCanvasDraw()
	{
		float canvasWidth = _cardCanvas.Size.X;
		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;
		Color gridColor = new(0.4f, 0.4f, 0.4f);

		for (int i = 1; i < 5; i++)
		{
			float y = i * rowHeight;
			_cardCanvas.DrawLine(new Vector2(0, y - 1), new Vector2(canvasWidth, y - 1), gridColor);
			_cardCanvas.DrawLine(new Vector2(0, y + 1), new Vector2(canvasWidth, y + 1), gridColor);
		}

		Color separatorColor = new(0.5f, 0.5f, 0.5f);
		_cardCanvas.DrawLine(new Vector2(canvasWidth - 1, 0), new Vector2(canvasWidth - 1, canvasHeight), separatorColor, 2);

		foreach (CombineRecipe edge in _edges)
		{
			Vector2 fromCenter = GetCardBottomCenter(edge.MainTypeId, rowHeight);
			Vector2 toCenter = GetCardTopCenter(edge.ResultTypeId, rowHeight);

			if (fromCenter == Vector2.Zero || toCenter == Vector2.Zero)
				continue;

			bool isCrossLine = edge.MaterialTypeId != edge.MainTypeId;
			Color lineColor = isCrossLine ? new Color(1f, 0.6f, 0f) : new Color(1f, 1f, 1f, 0.6f);

			Vector2 c1 = fromCenter + new Vector2(0, 30);
			Vector2 c2 = toCenter - new Vector2(0, 30);

			for (int i = 0; i < 19; i++)
			{
				float t1 = i / 19f;
				float t2 = (i + 1) / 19f;
				Vector2 p1 = CubicBezier(fromCenter, c1, c2, toCenter, t1);
				Vector2 p2 = CubicBezier(fromCenter, c1, c2, toCenter, t2);
				_cardCanvas.DrawLine(p1, p2, lineColor, 2);
			}

			DrawArrowHead(toCenter, c2, lineColor);
		}

		if (_dragMode == DragMode.CreateConnection && _dragSource != null)
		{
			Vector2 from = _dragSource.Position + new Vector2(CardWidth / 2, CardHeight);
			Vector2 to = _cardCanvas.GetLocalMousePosition();
			_cardCanvas.DrawLine(from, to, new Color(1f, 1f, 0f), 2);
		}
	}

	private void DrawArrowHead(Vector2 to, Vector2 control, Color color)
	{
		Vector2 dir = (to - control).Normalized();
		Vector2 perpendicular = new(-dir.Y, dir.X);
		float arrowSize = 8;
		Vector2 p1 = to - dir * arrowSize + perpendicular * (arrowSize * 0.5f);
		Vector2 p2 = to - dir * arrowSize - perpendicular * (arrowSize * 0.5f);

		_cardCanvas.DrawLine(to, p1, color, 2);
		_cardCanvas.DrawLine(to, p2, color, 2);
	}

	private async void ShowStatus(string message)
	{
		_statusBar.Text = message;
		await _root.ToSignal(_root.GetTree().CreateTimer(3), "timeout");
		_statusBar.Text = "就绪";
	}
}
