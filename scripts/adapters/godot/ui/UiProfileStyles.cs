// 游戏界面Profile分发：统一切换经典详细与粗描边动画样式
using Godot;

public enum UiProfileKind
{
	Detailed,
	Cartoon,
}

public static class UiProfileStyles
{
	public static void ApplyHud(CanvasLayer uiRoot, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyHud(uiRoot);
		else
			MedievalUiStyles.ApplyHud(uiRoot);
	}

	public static void ApplyHand(CanvasLayer uiRoot, Button useItemButton, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyHand(uiRoot, useItemButton);
		else
			MedievalUiStyles.ApplyHand(uiRoot, useItemButton);
	}

	public static void ApplyShop(CanvasLayer uiRoot, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyShop(uiRoot);
		else
			MedievalUiStyles.ApplyShop(uiRoot);
	}

	public static void ApplyCardButton(Button button, bool selected, bool occupied, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyCardButton(button, selected, occupied);
		else
			MedievalUiStyles.ApplyCardButton(button, selected, occupied);
	}

	public static void SetShopCircleHighlight(Panel panel, bool highlighted, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.SetShopCircleHighlight(panel, highlighted);
		else
			MedievalUiStyles.SetShopCircleHighlight(panel, highlighted);
	}
}