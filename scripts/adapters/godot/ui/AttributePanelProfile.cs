// 属性面板外观配置：定义经典详细与平面动画两套展示策略
using Godot;

public enum AttributePanelProfileKind
{
	Detailed,
	Cartoon,
}

public sealed class AttributeStatControl
{
	public PanelContainer Panel { get; }
	public Label ValueLabel { get; }

	public AttributeStatControl(PanelContainer panel, Label valueLabel)
	{
		Panel = panel;
		ValueLabel = valueLabel;
	}
}

public interface IAttributePanelProfile
{
	AttributePanelProfileKind Kind { get; }
	void Apply(AttributePanelView view);
	AttributeStatControl CreateStat(StatIconView.StatIconKind kind, string name, string text, string tooltip, string elementId);
	string FormatValue(StatIconView.StatIconKind kind, string text);
}

public static class AttributePanelProfiles
{
	private static readonly IAttributePanelProfile Detailed = new DetailedAttributePanelProfile();
	private static readonly IAttributePanelProfile Cartoon = new CartoonAttributePanelProfile();

	public static IAttributePanelProfile Get(AttributePanelProfileKind kind)
	{
		return kind == AttributePanelProfileKind.Cartoon ? Cartoon : Detailed;
	}
}

public sealed class DetailedAttributePanelProfile : IAttributePanelProfile
{
	private const float StatWidth = 430f;
	private const float StatNameWidth = 78f;
	private const float StatInfoWidth = 22f;
	private const int FontSize = 14;
	private static readonly Color Background = new(0.045f, 0.05f, 0.048f, 0.92f);
	private static readonly Color Border = new(0.24f, 0.2f, 0.13f, 1f);
	private static readonly Color NameColor = new(0.86f, 0.73f, 0.52f, 1f);
	private static readonly Color ValueColor = new(0.86f, 0.84f, 0.77f, 1f);

	public AttributePanelProfileKind Kind => AttributePanelProfileKind.Detailed;

	public void Apply(AttributePanelView view)
	{
		view.StatsGrid.Columns = 1;
		view.StatsGrid.AddThemeConstantOverride("h_separation", 0);
		view.StatsGrid.AddThemeConstantOverride("v_separation", 3);
		view.Panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		view.PortraitFrame.AddThemeStyleboxOverride("panel", CreatePortraitStyle());
		view.Badge.AddThemeStyleboxOverride("panel", CreateBadgeStyle());
		view.HealthBar.AddThemeStyleboxOverride("background", CreateHealthBackground());
		view.HealthBar.AddThemeStyleboxOverride("fill", CreateHealthFill());
		ApplyTextColors(view);
	}

	public AttributeStatControl CreateStat(StatIconView.StatIconKind kind, string name, string text, string tooltip, string elementId)
	{
		PanelContainer panel = CreateStatPanel(tooltip);
		HBoxContainer row = new();
		row.AddThemeConstantOverride("separation", 8);
		panel.AddChild(row);
		AddIcon(row, kind, tooltip);
		row.AddChild(CreateNameLabel(name));
		Label value = CreateValueLabel(text, tooltip);
		row.AddChild(value);
		AddElement(row, elementId);
		row.AddChild(CreateInfoLabel(tooltip));
		return new AttributeStatControl(panel, value);
	}

	public string FormatValue(StatIconView.StatIconKind kind, string text)
	{
		return text;
	}

	private static PanelContainer CreateStatPanel(string tooltip)
	{
		PanelContainer panel = new();
		panel.CustomMinimumSize = new Vector2(StatWidth, AttributePanelView.StatRowHeight);
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
		panel.TooltipText = tooltip;
		panel.AddThemeStyleboxOverride("panel", CreateStyle(Background, Border, 1, 2));
		return panel;
	}

	private static Label CreateNameLabel(string text)
	{
		Label label = CreateLabel(text, NameColor, FontSize);
		label.CustomMinimumSize = new Vector2(StatNameWidth, 0f);
		return label;
	}

	private static Label CreateValueLabel(string text, string tooltip)
	{
		Label label = CreateLabel(text, ValueColor, FontSize);
		label.TooltipText = tooltip;
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		return label;
	}

	private static Label CreateInfoLabel(string tooltip)
	{
		Label label = CreateLabel("ⓘ", NameColor, FontSize);
		label.TooltipText = tooltip;
		label.CustomMinimumSize = new Vector2(StatInfoWidth, 0f);
		label.HorizontalAlignment = HorizontalAlignment.Center;
		return label;
	}

	private static void ApplyTextColors(AttributePanelView view)
	{
		view.NameLabel.AddThemeColorOverride("font_color", new Color(0.93f, 0.82f, 0.62f, 1f));
		view.SubLabel.AddThemeColorOverride("font_color", new Color(0.68f, 0.7f, 0.65f, 1f));
		view.BadgeLabel.AddThemeColorOverride("font_color", new Color(0.96f, 0.76f, 0.34f, 1f));
		view.HealthValueLabel.AddThemeColorOverride("font_color", new Color(0.94f, 0.86f, 0.74f, 1f));
		view.ProgressLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.78f, 0.58f, 1f));
	}

	private static StyleBoxFlat CreatePanelStyle()
	{
		StyleBoxFlat style = CreateStyle(new Color(0.018f, 0.022f, 0.024f, 0.97f), new Color(0.63f, 0.46f, 0.22f, 1f), 2, 4);
		style.ShadowColor = new Color(0f, 0f, 0f, 0.72f);
		style.ShadowSize = 12;
		return style;
	}

	private static StyleBoxFlat CreatePortraitStyle()
	{
		return CreateStyle(new Color(0.025f, 0.031f, 0.034f, 1f), new Color(0.58f, 0.42f, 0.2f, 1f), 2, 3);
	}

	private static StyleBoxFlat CreateBadgeStyle()
	{
		StyleBoxFlat style = CreateStyle(new Color(0.11f, 0.075f, 0.03f, 1f), new Color(0.76f, 0.56f, 0.24f, 1f), 1, 8);
		SetMargins(style, 8f, 3f);
		return style;
	}

	private static StyleBoxFlat CreateHealthBackground()
	{
		return CreateStyle(new Color(0.07f, 0.045f, 0.035f, 1f), Colors.Transparent, 0, 5);
	}

	private static StyleBoxFlat CreateHealthFill()
	{
		return CreateStyle(new Color(0.72f, 0.055f, 0.055f, 1f), Colors.Transparent, 0, 5);
	}

	internal static StyleBoxFlat CreateStyle(Color background, Color border, int borderWidth, int radius)
	{
		StyleBoxFlat style = new();
		style.BgColor = background;
		style.BorderColor = border;
		style.SetBorderWidthAll(borderWidth);
		style.SetCornerRadiusAll(radius);
		return style;
	}

	internal static Label CreateLabel(string text, Color color, int fontSize)
	{
		Label label = new();
		label.Text = text;
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeFontSizeOverride("font_size", fontSize);
		return label;
	}

	internal static void AddIcon(Container row, StatIconView.StatIconKind kind, string tooltip)
	{
		StatIconView icon = new();
		icon.Configure(kind);
		icon.TooltipText = tooltip;
		row.AddChild(icon);
	}

	internal static void AddElement(Container row, string elementId)
	{
		if (string.IsNullOrEmpty(elementId))
			return;
		ElementIconView icon = new();
		icon.Configure(elementId);
		row.AddChild(icon);
	}

	internal static void SetMargins(StyleBoxFlat style, float horizontal, float vertical)
	{
		style.ContentMarginLeft = horizontal;
		style.ContentMarginRight = horizontal;
		style.ContentMarginTop = vertical;
		style.ContentMarginBottom = vertical;
	}
}

public sealed class CartoonAttributePanelProfile : IAttributePanelProfile
{
	private const float CardWidth = 138f;
	private const float CardHeight = 48f;
	private static readonly Color Ink = new(0.025f, 0.055f, 0.09f, 1f);
	private static readonly Color Navy = new(0.02f, 0.12f, 0.24f, 1f);
	private static readonly Color Coral = new(1f, 0.43f, 0.29f, 1f);
	private static readonly Color Cream = new(1f, 0.88f, 0.64f, 1f);
	private static readonly Color Yellow = new(1f, 0.76f, 0.05f, 1f);

	public AttributePanelProfileKind Kind => AttributePanelProfileKind.Cartoon;

	public void Apply(AttributePanelView view)
	{
		view.StatsGrid.Columns = 3;
		view.StatsGrid.AddThemeConstantOverride("h_separation", 6);
		view.StatsGrid.AddThemeConstantOverride("v_separation", 6);
		view.Panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		view.PortraitFrame.AddThemeStyleboxOverride("panel", CreatePortraitStyle());
		view.Badge.AddThemeStyleboxOverride("panel", CreateBadgeStyle());
		view.HealthBar.AddThemeStyleboxOverride("background", CreateHealthBackground());
		view.HealthBar.AddThemeStyleboxOverride("fill", CreateHealthFill());
		ApplyTextColors(view);
	}

	public AttributeStatControl CreateStat(StatIconView.StatIconKind kind, string name, string text, string tooltip, string elementId)
	{
		PanelContainer panel = CreateStatPanel(tooltip);
		HBoxContainer row = new();
		row.Alignment = BoxContainer.AlignmentMode.Center;
		row.AddThemeConstantOverride("separation", 5);
		panel.AddChild(row);
		DetailedAttributePanelProfile.AddIcon(row, kind, tooltip);
		VBoxContainer labels = CreateLabels(name, FormatValue(kind, text), tooltip);
		row.AddChild(labels);
		DetailedAttributePanelProfile.AddElement(row, elementId);
		Label value = labels.GetNode<Label>("Value");
		return new AttributeStatControl(panel, value);
	}

	public string FormatValue(StatIconView.StatIconKind kind, string text)
	{
		int separator = text.IndexOf('·');
		return separator < 0 ? text : text[..separator].Trim();
	}

	private static PanelContainer CreateStatPanel(string tooltip)
	{
		PanelContainer panel = new();
		panel.CustomMinimumSize = new Vector2(CardWidth, CardHeight);
		panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
		panel.TooltipText = tooltip;
		StyleBoxFlat style = DetailedAttributePanelProfile.CreateStyle(Cream, Ink, 4, 12);
		DetailedAttributePanelProfile.SetMargins(style, 5f, 4f);
		panel.AddThemeStyleboxOverride("panel", style);
		return panel;
	}

	private static VBoxContainer CreateLabels(string name, string text, string tooltip)
	{
		VBoxContainer labels = new();
		labels.AddThemeConstantOverride("separation", 0);
		Label nameLabel = DetailedAttributePanelProfile.CreateLabel(name, Ink, 11);
		Label valueLabel = DetailedAttributePanelProfile.CreateLabel(text, Ink, 14);
		valueLabel.Name = "Value";
		valueLabel.TooltipText = tooltip;
		valueLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		labels.AddChild(nameLabel);
		labels.AddChild(valueLabel);
		return labels;
	}

	private static void ApplyTextColors(AttributePanelView view)
	{
		view.NameLabel.AddThemeColorOverride("font_color", Colors.White);
		view.SubLabel.AddThemeColorOverride("font_color", Cream);
		view.BadgeLabel.AddThemeColorOverride("font_color", Ink);
		view.HealthValueLabel.AddThemeColorOverride("font_color", Colors.White);
		view.ProgressLabel.AddThemeColorOverride("font_color", Cream);
	}

	private static StyleBoxFlat CreatePanelStyle()
	{
		StyleBoxFlat style = DetailedAttributePanelProfile.CreateStyle(Navy, Coral, 10, 18);
		style.ShadowColor = Ink;
		style.ShadowSize = 7;
		return style;
	}

	private static StyleBoxFlat CreatePortraitStyle()
	{
		return DetailedAttributePanelProfile.CreateStyle(Cream, Ink, 5, 14);
	}

	private static StyleBoxFlat CreateBadgeStyle()
	{
		StyleBoxFlat style = DetailedAttributePanelProfile.CreateStyle(Yellow, Ink, 4, 14);
		DetailedAttributePanelProfile.SetMargins(style, 8f, 3f);
		return style;
	}

	private static StyleBoxFlat CreateHealthBackground()
	{
		return DetailedAttributePanelProfile.CreateStyle(Navy, Ink, 3, 9);
	}

	private static StyleBoxFlat CreateHealthFill()
	{
		return DetailedAttributePanelProfile.CreateStyle(new Color(0.05f, 0.78f, 0.2f, 1f), Ink, 3, 9);
	}
}