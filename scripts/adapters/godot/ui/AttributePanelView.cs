// 单位与建筑属性面板复用的节点视图和紧凑布局
using Godot;

public sealed class AttributePanelView
{
	private const float PanelWidth = 460f;
	private const float PanelLeft = 18f;
	private const float PanelBottom = 1062f;
	private const float MinimumPanelHeight = 230f;
	private const float VerticalMargins = 16f;
	private const float HeaderHeight = 82f;
	private const float HealthHeight = 22f;
	private const float BottomHeight = 60f;
	public const float StatRowHeight = 24f;
	private const float SectionSeparation = 6f;
	private const float StatSeparation = 3f;

	public readonly Panel Panel;
	public readonly TextureRect PortraitRect;
	public readonly Label NameLabel;
	public readonly Label SubLabel;
	public readonly Label BadgeLabel;
	public readonly ProgressBar HealthBar;
	public readonly Label HealthValueLabel;
	public readonly GridContainer StatsGrid;
	public readonly Control BottomArea;
	public readonly HBoxContainer SkillsRow;
	public readonly VBoxContainer ProgressBox;
	public readonly Label ProgressLabel;
	public readonly ProgressBar ProgressBar;

	public AttributePanelView(Panel panel)
	{
		Panel = panel;
		PortraitRect = panel.GetNode<TextureRect>("Margin/Content/Header/PortraitFrame/PortraitRect");
		NameLabel = panel.GetNode<Label>("Margin/Content/Header/HeaderText/NameLabel");
		SubLabel = panel.GetNode<Label>("Margin/Content/Header/HeaderText/SubLabel");
		BadgeLabel = panel.GetNode<Label>("Margin/Content/Header/Badge/BadgeLabel");
		HealthBar = panel.GetNode<ProgressBar>("Margin/Content/HealthRow/HealthBar");
		HealthValueLabel = panel.GetNode<Label>("Margin/Content/HealthRow/HealthValueLabel");
		StatsGrid = panel.GetNode<GridContainer>("Margin/Content/StatsGrid");
		BottomArea = panel.GetNode<Control>("Margin/Content/BottomArea");
		SkillsRow = BottomArea.GetNode<HBoxContainer>("SkillsRow");
		ProgressBox = BottomArea.GetNode<VBoxContainer>("ProgressBox");
		ProgressLabel = ProgressBox.GetNode<Label>("ProgressLabel");
		ProgressBar = ProgressBox.GetNode<ProgressBar>("ProgressBar");
	}

	public void Show()
	{
		Panel.Visible = true;
	}

	public void Hide()
	{
		Panel.Visible = false;
	}

	public void UpdateLayout()
	{
		int statRows = StatsGrid.GetChildCount();
		float statsHeight = GetStatsHeight(statRows);
		StatsGrid.CustomMinimumSize = new Vector2(0f, statsHeight);
		float height = GetPanelHeight(statsHeight, BottomArea.Visible);
		Panel.Size = new Vector2(PanelWidth, height);
		Panel.Position = new Vector2(PanelLeft, PanelBottom - height);
	}

	private static float GetStatsHeight(int rows)
	{
		if (rows == 0)
			return 0f;
		return rows * StatRowHeight + (rows - 1) * StatSeparation;
	}

	private static float GetPanelHeight(float statsHeight, bool hasBottom)
	{
		int sectionGaps = hasBottom ? 3 : 2;
		float bottomHeight = hasBottom ? BottomHeight : 0f;
		float contentHeight = VerticalMargins + HeaderHeight + HealthHeight + statsHeight
			+ sectionGaps * SectionSeparation + bottomHeight;
		return Mathf.Max(MinimumPanelHeight, contentHeight);
	}
}
