using Godot;
using System;

public sealed class SettingsUiController
{
	private readonly Control _root;
	private readonly OptionButton _windowModeOption;
	private readonly OptionButton _resolutionOption;
	private readonly Button _applyButton;
	private readonly Button _backButton;

	public bool IsOpen { get; private set; }
	public event Action<bool> OpenChanged;

	public SettingsUiController(Control root)
	{
		_root = root;
		_windowModeOption = root.GetNode<OptionButton>("Panel/WindowModeOption");
		_resolutionOption = root.GetNode<OptionButton>("Panel/ResolutionOption");
		_applyButton = root.GetNode<Button>("Panel/ApplyButton");
		_backButton = root.GetNode<Button>("Panel/BackButton");

		_windowModeOption.Clear();
		_windowModeOption.AddItem("窗口化", (int)DisplayWindowMode.Windowed);
		_windowModeOption.AddItem("无边框全屏", (int)DisplayWindowMode.BorderlessFullscreen);

		_resolutionOption.Clear();
		foreach (Vector2I resolution in GameConfig.OutputResolutions)
			_resolutionOption.AddItem($"{resolution.X} × {resolution.Y}", _resolutionOption.ItemCount);

		_windowModeOption.ItemSelected += OnWindowModeSelected;
		_applyButton.Pressed += OnApplyPressed;
		_backButton.Pressed += OnBackPressed;

		_root.Visible = false;
	}

	public void Dispose()
	{
		_windowModeOption.ItemSelected -= OnWindowModeSelected;
		_applyButton.Pressed -= OnApplyPressed;
		_backButton.Pressed -= OnBackPressed;
	}

	public void Open()
	{
		if (IsOpen)
			return;

		IsOpen = true;
		_root.Visible = true;
		SyncFromSaved();
		OpenChanged?.Invoke(true);
	}

	public bool Close()
	{
		if (!IsOpen)
			return false;

		IsOpen = false;
		_root.Visible = false;
		OpenChanged?.Invoke(false);
		return true;
	}

	private void SyncFromSaved()
	{
		DisplaySettingsManager settings = DisplaySettingsManager.Instance;
		_windowModeOption.Select(_windowModeOption.GetItemIndex((int)settings.WindowMode));
		_resolutionOption.Select(settings.FindResolutionIndex(settings.OutputResolution));
		UpdateResolutionEnabled();
	}

	private void OnWindowModeSelected(long _)
	{
		UpdateResolutionEnabled();
	}

	private void UpdateResolutionEnabled()
	{
		bool windowed = (DisplayWindowMode)_windowModeOption.GetSelectedId() == DisplayWindowMode.Windowed;
		_resolutionOption.Disabled = !windowed;
	}

	private void OnApplyPressed()
	{
		DisplayWindowMode mode = (DisplayWindowMode)_windowModeOption.GetSelectedId();
		Vector2I resolution = GameConfig.OutputResolutions[_resolutionOption.Selected];
		DisplaySettingsManager.Instance.SaveAndApply(mode, resolution);
	}

	private void OnBackPressed() => Close();
}
