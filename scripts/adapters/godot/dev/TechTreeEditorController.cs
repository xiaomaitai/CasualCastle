// 科技树编辑器：节点、连线与数据读写
using System;
using System.Collections.Generic;
using System.Linq;
using CasualCastle.Domain.Building;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public class TechTreeEditorController
{
	private readonly Control _root;
	private readonly Button _prevFactionButton;
	private readonly Label _factionNameLabel;
	private readonly Button _nextFactionButton;
	private readonly Button _saveButton;
	private readonly Button _closeButton;
	private readonly Control _cardCanvas;
	private readonly Control _libraryPanel;
	private readonly VBoxContainer _libraryList;
	private readonly Button _buildingEditorButton;
	private readonly Label _statusBar;

	private readonly ITechTreeRepository _repo;
	private List<FactionDef> _factions = new();
	private List<TechTreeNode> _nodes = new();
	private List<TechTreeEdge> _edges = new();
	private List<BuildingTypeSummary> _inventory = new();
	private string _currentFactionId = "";
	private int _currentFactionIndex = 0;

	private enum DragMode { None, MoveCard, CreateConnection, LibraryToCanvas, CanvasToLibrary }
	private DragMode _dragMode;
	private Panel _dragSource;
	private Panel _dragPreview;
	private Vector2 _dragOffset;
	private bool _dragActive;

	private const float CardWidth = 120;
	private const float CardHeight = 56;
	private const float ColSpacing = 140;
	private const int DefaultBuildingValue = 1;
	private const string DevToolsMenuScene = "res://scenes/ui/dev_tools/dev_tools_menu.tscn";
	private const string BuildingEditorScene = "res://scenes/ui/dev_tools/building_editor.tscn";

	public TechTreeEditorController(Control root, ITechTreeRepository repo)
	{
		_root = root;
		_prevFactionButton = root.GetNode<Button>("TopBar/PrevFactionButton");
		_factionNameLabel = root.GetNode<Label>("TopBar/FactionNameLabel");
		_nextFactionButton = root.GetNode<Button>("TopBar/NextFactionButton");
		_saveButton = root.GetNode<Button>("TopBar/SaveButton");
		_closeButton = root.GetNode<Button>("TopBar/CloseButton");
		_cardCanvas = root.GetNode<Control>("EditorArea/CanvasScroll/CardCanvas");
		_libraryPanel = root.GetNode<Control>("EditorArea/LibraryPanel");
		_libraryList = root.GetNode<VBoxContainer>("EditorArea/LibraryPanel/LibraryScroll/LibraryList");
		_buildingEditorButton = root.GetNode<Button>("TopBar/BuildingEditorButton");
		_statusBar = root.GetNode<Label>("StatusBar");

		_repo = repo;

		_prevFactionButton.Pressed += OnPrevFactionPressed;
		_nextFactionButton.Pressed += OnNextFactionPressed;
		_saveButton.Pressed += OnSavePressed;
		_closeButton.Pressed += OnClosePressed;
		_buildingEditorButton.Pressed += OnBuildingEditorPressed;
		_cardCanvas.Draw += OnCardCanvasDraw;
		_cardCanvas.GuiInput += OnCardCanvasGuiInput;
	}

	public void LoadInitialData()
	{
		_factions = _repo.LoadFactions();
		if (_factions.Count == 0)
			return;
		int startIndex = 0;
		if (!string.IsNullOrEmpty(DevEditorContext.CurrentFactionId))
		{
			int contextIndex = _factions.FindIndex(f => f.Id == DevEditorContext.CurrentFactionId);
			if (contextIndex >= 0)
				startIndex = contextIndex;
		}
		_currentFactionIndex = startIndex;
		LoadFactionAtIndex(startIndex);
	}

	public void Dispose()
	{
		_prevFactionButton.Pressed -= OnPrevFactionPressed;
		_nextFactionButton.Pressed -= OnNextFactionPressed;
		_saveButton.Pressed -= OnSavePressed;
		_closeButton.Pressed -= OnClosePressed;
		_buildingEditorButton.Pressed -= OnBuildingEditorPressed;
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
				Vector2 globalPos = _cardCanvas.GetGlobalMousePosition();
				_dragPreview.GlobalPosition = globalPos - _dragOffset;
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

	private void OnPrevFactionPressed()
	{
		if (_currentFactionIndex > 0)
		{
			_currentFactionIndex--;
			SaveAll();
			GD.Print($"[TechTreeEditor] 切换到上一个派系, 索引={_currentFactionIndex}, ID={_factions[_currentFactionIndex].Id}");
			LoadFactionAtIndex(_currentFactionIndex);
		}
	}

	private void OnNextFactionPressed()
	{
		if (_currentFactionIndex < _factions.Count - 1)
		{
			_currentFactionIndex++;
			SaveAll();
			GD.Print($"[TechTreeEditor] 切换到下一个派系, 索引={_currentFactionIndex}, ID={_factions[_currentFactionIndex].Id}");
			LoadFactionAtIndex(_currentFactionIndex);
		}
	}

	private void LoadFactionAtIndex(int index)
	{
		GD.Print($"[TechTreeEditor] LoadFactionAtIndex index={index}, factionId={_factions[index].Id}, 切换前_nodes.Count={_nodes.Count}");
		_currentFactionIndex = index;
		FactionDef faction = _factions[index];
		_currentFactionId = faction.Id;
		_factionNameLabel.Text = faction.DisplayName;
		_nodes = _repo.LoadNodes(faction.Id);
		_edges = LoadCanvasEdges(faction.Id);
		int reflowedCount = ReflowOverlappingNodes();
		GD.Print($"[TechTreeEditor] 加载完成: factionId={faction.Id}, nodes={_nodes.Count}, edges={_edges.Count}, 重叠重排={reflowedCount}");
		LayoutCards();
		RefreshLibrary();
		string reflowSuffix = reflowedCount > 0 ? $"（自动重排 {reflowedCount} 个重叠卡片）" : "";
		ShowStatus($"已加载 {_nodes.Count} 个节点，{_edges.Count} 条连线{reflowSuffix}");
	}

	private int ReflowOverlappingNodes()
	{
		int reflowedCount = 0;
		foreach (IGrouping<int, TechTreeNode> tierGroup in _nodes.GroupBy(n => n.Tier))
		{
			List<int> usedCols = new();
			foreach (TechTreeNode node in tierGroup.OrderBy(n => n.Col).ThenBy(n => n.TypeId))
			{
				if (usedCols.Contains(node.Col))
				{
					node.Col = GetNextFreeColumn(usedCols, node.Col);
					reflowedCount++;
				}
				usedCols.Add(node.Col);
			}
		}
		return reflowedCount;
	}

	private static int GetNextFreeColumn(List<int> usedCols, int startCol)
	{
		int nextCol = startCol;
		while (usedCols.Contains(nextCol))
			nextCol++;
		return nextCol;
	}

	private void LayoutCards()
	{
		foreach (Node child in _cardCanvas.GetChildren())
		{
			if (child is Panel panel && panel.GetMeta("type_id").AsString() != null)
				child.QueueFree();
		}

		_cardCanvas.CustomMinimumSize = new Vector2(GetCanvasContentWidth(), 0);

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

	private float GetCanvasContentWidth()
	{
		int maxCol = 0;
		foreach (TechTreeNode node in _nodes)
		{
			if (node.Col > maxCol)
				maxCol = node.Col;
		}
		return (maxCol + 2) * ColSpacing + 20;
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
		bool hasIncomingEdge = _edges.Exists(e => e.ToTypeId == node.TypeId);
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

		_inventory = _repo.LoadInventory(_currentFactionId);
		int availableCount = 0;
		GD.Print($"[TechTreeEditor] RefreshLibrary: 库存总数={_inventory.Count}, 当前派系节点数={_nodes.Count}");

		foreach (BuildingTypeSummary type in _inventory)
		{
			if (_nodes.Exists(n => n.TypeId == type.TypeId))
				continue;
			availableCount++;
			Panel item = CreateLibraryItem(type);
			_libraryList.AddChild(item);
		}
		GD.Print($"[TechTreeEditor] RefreshLibrary 完成: 可用库存={availableCount}, 已放置={_inventory.Count - availableCount}");
	}

	private Panel CreateLibraryItem(BuildingTypeSummary type)
	{
		Panel panel = new()
		{
			CustomMinimumSize = new Vector2(180, 36),
			Size = new Vector2(180, 36),
			MouseFilter = Control.MouseFilterEnum.Stop
		};

		StyleBoxFlat styleBox = new();
		styleBox.BgColor = new Color(0.2f, 0.4f, 0.55f);
		panel.AddThemeStyleboxOverride("panel", styleBox);

		Label label = new()
		{
			Text = type.DisplayName,
			Position = new Vector2(4, 8),
			Size = new Vector2(172, 20),
			HorizontalAlignment = HorizontalAlignment.Center,
			Modulate = new Color(1f, 1f, 1f)
		};
		panel.AddChild(label);

		panel.SetMeta("type_id", type.TypeId);

		panel.GuiInput += e => OnLibraryItemGuiInput(panel, e);

		return panel;
	}

	private void OnCardGuiInput(Panel card, InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				_dragMode = DragMode.MoveCard;
				_dragSource = card;
				_dragActive = true;
				_dragOffset = _cardCanvas.GetGlobalMousePosition() - card.GlobalPosition;
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
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.DoubleClick)
			{
				Vector2 clickPos = _cardCanvas.GetLocalMousePosition();
				TryDeleteEdgeAt(clickPos);
				return;
			}

			if (mouseButton.ButtonIndex == MouseButton.Right && !_dragActive)
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
		int newCol = Math.Max(0, (int)Math.Round((_cardCanvas.GetGlobalMousePosition().X - _dragOffset.X - _cardCanvas.GlobalPosition.X - 10) / ColSpacing));

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
		List<TechTreeEdge> invalidEdges = _edges.Where(e =>
		{
			if (e.FromTypeId == movedNode.TypeId)
			{
				TechTreeNode resultNode = _nodes.Find(n => n.TypeId == e.ToTypeId);
				return resultNode != null && resultNode.Tier <= movedNode.Tier;
			}
			if (e.ToTypeId == movedNode.TypeId)
			{
				TechTreeNode sourceNode = _nodes.Find(n => n.TypeId == e.FromTypeId);
				return sourceNode != null && movedNode.Tier <= sourceNode.Tier;
			}
			return false;
		}).ToList();

		foreach (TechTreeEdge edge in invalidEdges)
			_edges.Remove(edge);

		if (invalidEdges.Count > 0)
			ShowStatus($"已移除 {invalidEdges.Count} 条无效连线");
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

			if (_edges.Exists(e => e.FromTypeId == fromId && e.ToTypeId == toId))
			{
				ShowStatus("连线已存在");
				CancelDrag();
				return;
			}

			_edges.Add(new TechTreeEdge { FromTypeId = fromId, ToTypeId = toId });
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

		BuildingTypeSummary typeInfo = _inventory.Find(t => t.TypeId == typeId);
		string displayName = typeInfo?.DisplayName ?? typeId;

		TechTreeNode newNode = new()
		{
			TypeId = typeId,
			FactionId = _currentFactionId,
			Tier = tier,
			Col = maxCol + 1,
			ShopAvailable = typeInfo?.ShopAvailable ?? false,
			GoldCost = typeInfo?.GoldCost ?? 0,
			ShopWeight = typeInfo?.ShopWeight ?? 1,
			DisplayName = displayName,
			UnitTypeId = typeInfo?.UnitTypeId ?? typeId,
			MaxHealth = typeInfo?.MaxHealth ?? 200,
			SpawnInterval = typeInfo?.SpawnInterval ?? 10.0f,
			Value = typeInfo?.Value ?? DefaultBuildingValue
		};

		_nodes.Add(newNode);
		LayoutCards();
		RefreshLibrary();
		CancelDrag();
		ShowStatus($"已放置: {displayName} (T{tier})，已从库存扣除");
	}

	private void CanvasToLibrary(Panel card)
	{
		string typeId = card.GetMeta("type_id").AsString();

		_edges.RemoveAll(e => e.FromTypeId == typeId || e.ToTypeId == typeId);

		_nodes.RemoveAll(n => n.TypeId == typeId);
		LayoutCards();
		RefreshLibrary();
		ShowStatus($"已移除: {typeId}（已回到库存）");
	}

	private void TryDeleteEdgeAt(Vector2 canvasPos)
	{
		float canvasHeight = _cardCanvas.Size.Y;
		float rowHeight = canvasHeight / 5;
		float tolerance = 10f;

		TechTreeEdge toDelete = null;
		foreach (TechTreeEdge edge in _edges)
		{
			Vector2 fromCenter = GetCardCenter(edge.FromTypeId, rowHeight);
			Vector2 toCenter = GetCardCenter(edge.ToTypeId, rowHeight);
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
			_edges.Remove(toDelete);
			LayoutCards();
			ShowStatus($"连线已删除: {toDelete.FromTypeId} → {toDelete.ToTypeId}");
		}
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

	private Vector2 GetCardCenter(string typeId, float rowHeight)
	{
		TechTreeNode node = _nodes.Find(n => n.TypeId == typeId);
		if (node == null)
			return Vector2.Zero;

		float x = node.Col * ColSpacing + 10 + CardWidth / 2;
		float y = (4 - (node.Tier - 1)) * rowHeight + rowHeight / 2;
		return new Vector2(x, y);
	}

	private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		float u = 1 - t;
		return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
	}

	private void OnSavePressed()
	{
		SaveAll();
		ShowStatus("保存成功");
	}

	private void SaveAll()
	{
		_repo.SaveNodes(_currentFactionId, _nodes);
		SaveEdgesByIncomingCount();
		_repo.SyncToGameTables(_currentFactionId);
	}

	private void SaveEdgesByIncomingCount()
	{
		List<CombineRecipe> combineEdges = new();
		List<UpgradeRecipe> upgradeEdges = new();

		foreach (IGrouping<string, TechTreeEdge> group in _edges.GroupBy(e => e.ToTypeId))
		{
			List<TechTreeEdge> incoming = group.ToList();
			if (incoming.Count == 1)
			{
				upgradeEdges.Add(new UpgradeRecipe
				{
					SourceTypeId = incoming[0].FromTypeId,
					ResultTypeId = group.Key
				});
				continue;
			}
			AddCombineRecipes(combineEdges, incoming, group.Key);
		}

		_repo.SaveEdges(_currentFactionId, combineEdges);
		_repo.SaveUpgradeEdges(_currentFactionId, upgradeEdges);
	}

	private static void AddCombineRecipes(List<CombineRecipe> combineEdges, List<TechTreeEdge> incoming, string resultTypeId)
	{
		string mainTypeId = incoming[0].FromTypeId;
		HashSet<string> materialTypes = new();
		foreach (TechTreeEdge edge in incoming)
		{
			if (edge.FromTypeId == mainTypeId || !materialTypes.Add(edge.FromTypeId))
				continue;
			combineEdges.Add(new CombineRecipe
			{
				MainTypeId = mainTypeId,
				MaterialTypeId = edge.FromTypeId,
				MaterialCount = 1,
				ResultTypeId = resultTypeId
			});
		}
	}

	private void OnClosePressed()
	{
		_root.GetTree().ChangeSceneToFile(DevToolsMenuScene);
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

		foreach (TechTreeEdge edge in _edges)
		{
			Vector2 fromCenter = GetCardCenter(edge.FromTypeId, rowHeight);
			Vector2 toCenter = GetCardCenter(edge.ToTypeId, rowHeight);

			if (fromCenter == Vector2.Zero || toCenter == Vector2.Zero)
				continue;

			bool isUpgradeEdge = _edges.Count(e => e.ToTypeId == edge.ToTypeId) == 1;
			Color lineColor = isUpgradeEdge ? new Color(0.2f, 0.8f, 0.9f) : new Color(1f, 0.6f, 0f);

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

		}

		if (_dragMode == DragMode.CreateConnection && _dragSource != null)
		{
			Vector2 from = _dragSource.Position + new Vector2(CardWidth / 2, CardHeight / 2);
			Vector2 to = _cardCanvas.GetLocalMousePosition();
			_cardCanvas.DrawLine(from, to, new Color(1f, 1f, 0f), 2);
		}
	}

	private List<TechTreeEdge> LoadCanvasEdges(string factionId)
	{
		List<TechTreeEdge> edges = new();
		foreach (CombineRecipe recipe in _repo.LoadEdges(factionId))
		{
			edges.Add(new TechTreeEdge { FromTypeId = recipe.MainTypeId, ToTypeId = recipe.ResultTypeId });
			if (recipe.MaterialTypeId != recipe.MainTypeId)
				edges.Add(new TechTreeEdge { FromTypeId = recipe.MaterialTypeId, ToTypeId = recipe.ResultTypeId });
		}
		foreach (UpgradeRecipe recipe in _repo.LoadUpgradeEdges(factionId))
			edges.Add(new TechTreeEdge { FromTypeId = recipe.SourceTypeId, ToTypeId = recipe.ResultTypeId });
		return edges;
	}

	private void OnBuildingEditorPressed()
	{
		SaveAll();
		DevEditorContext.CurrentFactionId = _currentFactionId;
		_root.GetTree().ChangeSceneToFile(BuildingEditorScene);
	}

	private async void ShowStatus(string message)
	{
		_statusBar.Text = message;
		await _root.ToSignal(_root.GetTree().CreateTimer(3), "timeout");
		_statusBar.Text = "就绪";
	}
}
