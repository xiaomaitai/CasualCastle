// UI 交互状态投影：结算与模态的统一输入屏蔽视图
using System;

public sealed class UiInteractionState
{
	public bool GameOver { get; private set; }
	public bool PauseOpen { get; private set; }
	public bool SettingsOpen { get; private set; }
	public bool ShopOpen { get; private set; }
	public bool InputBlocked => GameOver || PauseOpen || SettingsOpen;

	public event Action Changed;

	public void Update(bool gameOver, bool pauseOpen, bool settingsOpen, bool shopOpen)
	{
		bool changed = gameOver != GameOver || pauseOpen != PauseOpen || settingsOpen != SettingsOpen || shopOpen != ShopOpen;
		if (!changed)
			return;
		GameOver = gameOver;
		PauseOpen = pauseOpen;
		SettingsOpen = settingsOpen;
		ShopOpen = shopOpen;
		Changed?.Invoke();
	}
}
