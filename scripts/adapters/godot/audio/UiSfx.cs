// UI 音效绑定工具：为按钮统一挂接点击音效。
using Godot;
using System.Collections.Generic;

public static class UiSfx
{
	public static void Bind(Node root)
	{
		foreach (Button button in FindButtons(root))
			BindButton(button);
	}

	public static void BindButton(Button button)
	{
		button.Pressed += () => Sfx.Play(SfxEvents.ButtonClick);
	}

	private static IEnumerable<Button> FindButtons(Node root)
	{
		List<Button> buttons = new();
		CollectButtons(root, buttons);
		return buttons;
	}

	private static void CollectButtons(Node node, List<Button> buttons)
	{
		if (node is Button button)
			buttons.Add(button);

		foreach (Node child in node.GetChildren())
			CollectButtons(child, buttons);
	}
}
