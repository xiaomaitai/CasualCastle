// 酒馆漫画式对话气泡：选中酒馆时显示等待冒险者头像，点击头像提前弹出
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System.Collections.Generic;

public partial class TavernBubble : Node2D
{
	private const int SlotSize = 56;
	private const int SlotGap = 8;
	private const int Padding = 12;
	private const int TailHeight = 12;
	private const int PortraitInset = 5;
	private const int CornerRadius = 10;
	private const int BubbleLift = 10;

	private readonly List<IUnitHandle> _waiting = new();
	private readonly List<Rect2> _slotRects = new();
	private readonly Dictionary<string, Texture2D> _portraitCache = new();
	private Building _building;
	private Vector2 _bubbleSize;
	private string _signature = "";

	public void Process(Building building, TavernPartyService tavernParty)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || building.IsDestroyed
			|| !TavernPartyService.IsTavernBuilding(building.TypeId))
		{
			HideBubble();
			return;
		}

		IReadOnlyList<IUnitHandle> waiting = tavernParty.GetWaitingUnits(building.Bridge);
		if (waiting.Count == 0)
		{
			HideBubble();
			return;
		}

		string signature = BuildSignature(waiting);
		if (_building != building || signature != _signature)
		{
			AttachTo(building);
			_building = building;
			_waiting.Clear();
			_waiting.AddRange(waiting);
			_signature = signature;
			LayoutBubble(building);
			Visible = true;
			QueueRedraw();
		}
	}

	public bool HandleInput(InputEvent @event, TavernPartyService tavernParty)
	{
		if (!Visible || _building == null || _waiting.Count == 0)
			return false;
		if (tavernParty == null)
			return false;
		if (@event is not InputEventMouseButton mouseButton
			|| mouseButton.ButtonIndex != MouseButton.Left
			|| !mouseButton.Pressed)
			return false;

		Vector2 worldPoint = GameCoordinatesAdapter.ScreenToWorld(GetViewport(), mouseButton.GlobalPosition);
		Vector2 local = ToLocal(worldPoint);
		for (int i = 0; i < _slotRects.Count; i++)
		{
			if (i >= _waiting.Count)
				break;
			if (!_slotRects[i].HasPoint(local))
				continue;
			tavernParty.Eject(_waiting[i]);
			Sfx.Play(SfxEvents.ButtonClick);
			return true;
		}
		return false;
	}

	public void HideBubble()
	{
		if (!Visible && _building == null)
			return;
		_building = null;
		_waiting.Clear();
		_signature = "";
		Visible = false;
	}

	private void AttachTo(Building building)
	{
		Node parent = GetParent();
		Node effects = building.GetNodeOrNull<Node2D>("Effects");
		if (effects == null)
			effects = building;
		if (parent == effects)
			return;
		if (parent != null)
			parent.RemoveChild(this);
		effects.AddChild(this);
	}

	private void LayoutBubble(Building building)
	{
		int slotCount = TavernPartyService.GetPartyLimit(building.TypeId);
		_bubbleSize = new Vector2(
			Padding * 2 + slotCount * SlotSize + (slotCount - 1) * SlotGap,
			Padding * 2 + SlotSize + TailHeight);
		Position = new Vector2(0f,
			-(building.GetBuildingSize().Y * 0.5f + _bubbleSize.Y * 0.5f + BubbleLift));
		_slotRects.Clear();
		for (int i = 0; i < slotCount; i++)
		{
			float x = Padding + i * (SlotSize + SlotGap);
			_slotRects.Add(new Rect2(x, Padding, SlotSize, SlotSize));
		}
	}

	public override void _Draw()
	{
		if (_building == null)
			return;
		DrawBubbleBackground();
		DrawSlots();
	}

	private void DrawBubbleBackground()
	{
		Color fill = new Color(0.98f, 0.95f, 0.86f, 0.97f);
		Color outline = new Color(0.35f, 0.27f, 0.18f, 1f);
		Rect2 rect = new Rect2(Vector2.Zero, _bubbleSize);
		DrawRoundedRect(rect, CornerRadius, fill);
		DrawRoundedRectOutline(rect, CornerRadius, outline, 3f);
		DrawTail(outline, fill);
	}

	private void DrawTail(Color outline, Color fill)
	{
		float centerX = _bubbleSize.X * 0.5f;
		float topY = _bubbleSize.Y - TailHeight;
		Vector2[] points = new Vector2[]
		{
			new Vector2(centerX - TailHeight, topY),
			new Vector2(centerX + TailHeight, topY),
			new Vector2(centerX, _bubbleSize.Y + TailHeight)
		};
		DrawColoredPolygon(points, fill);
		DrawLine(points[0], points[2], outline, 3f);
		DrawLine(points[2], points[1], outline, 3f);
	}

	private void DrawSlots()
	{
		Color fill = new Color(0.92f, 0.88f, 0.78f, 1f);
		Color border = new Color(0.35f, 0.27f, 0.18f, 1f);
		for (int i = 0; i < _slotRects.Count; i++)
		{
			Rect2 slot = _slotRects[i];
			DrawRoundedRect(slot, 6, fill);
			DrawRoundedRectOutline(slot, 6, border, 2f);
			if (i >= _waiting.Count)
				continue;
			Texture2D portrait = LoadPortrait(_waiting[i].TypeId);
			if (portrait == null)
				continue;
			DrawPortrait(portrait, slot);
		}
	}

	private void DrawPortrait(Texture2D portrait, Rect2 slot)
	{
		Rect2 inner = new Rect2(
			slot.Position + new Vector2(PortraitInset, PortraitInset),
			new Vector2(slot.Size.X - PortraitInset * 2, slot.Size.Y - PortraitInset * 2));
		float scale = Mathf.Min(inner.Size.X / portrait.GetWidth(), inner.Size.Y / portrait.GetHeight());
		Vector2 drawSize = new Vector2(portrait.GetWidth() * scale, portrait.GetHeight() * scale);
		Vector2 drawPos = inner.Position + (inner.Size - drawSize) * 0.5f;
		DrawTextureRect(portrait, new Rect2(drawPos, drawSize), false, new Color(1f, 1f, 1f, 1f));
	}

	private Texture2D LoadPortrait(string typeId)
	{
		if (_portraitCache.TryGetValue(typeId, out Texture2D cached))
			return cached;
		Texture2D portrait = AttributePanelController.LoadUnitPortrait(typeId)
			?? AttributePanelController.LoadPlaceholder();
		_portraitCache[typeId] = portrait;
		return portrait;
	}

	private void DrawRoundedRect(Rect2 rect, float radius, Color color)
	{
		DrawRect(rect, color);
		DrawCircle(new Vector2(rect.Position.X + radius, rect.Position.Y + radius), radius, color);
		DrawCircle(new Vector2(rect.End.X - radius, rect.Position.Y + radius), radius, color);
		DrawCircle(new Vector2(rect.Position.X + radius, rect.End.Y - radius), radius, color);
		DrawCircle(new Vector2(rect.End.X - radius, rect.End.Y - radius), radius, color);
	}

	private void DrawRoundedRectOutline(Rect2 rect, float radius, Color color, float width)
	{
		DrawRect(new Rect2(rect.Position - new Vector2(width * 0.5f, width * 0.5f),
			rect.Size + new Vector2(width, width)), color, false, width);
	}

	private static string BuildSignature(IReadOnlyList<IUnitHandle> waiting)
	{
		string signature = "";
		foreach (IUnitHandle unit in waiting)
			signature += unit.TypeId + ";";
		return signature;
	}
}