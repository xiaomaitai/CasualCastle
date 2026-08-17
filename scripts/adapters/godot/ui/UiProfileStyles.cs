// 游戏界面Profile分发：统一切换经典详细与粗描边动画样式
using Godot;

public enum UiProfileKind
{
	Detailed,
	Cartoon,
}

public static class UiProfileStyles
{
	public static void ApplyHud(Control hud, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyHud(hud);
		else
			MedievalUiStyles.ApplyHud(hud);
	}

	public static void ApplyHand(Control handPanel, Button useItemButton, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyHand(handPanel, useItemButton);
		else
			MedievalUiStyles.ApplyHand(handPanel, useItemButton);
	}

	public static void ApplyShop(Control shopPanel, UiProfileKind kind)
	{
		if (kind == UiProfileKind.Cartoon)
			CartoonUiStyles.ApplyShop(shopPanel);
		else
			MedievalUiStyles.ApplyShop(shopPanel);
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