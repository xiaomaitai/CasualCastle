using CasualCastle.Adapters.Godot;
using Godot;
using System;

public sealed class SettingsUiController
{
	private readonly Control _root;
	private readonly OptionButton _windowModeOption;
	private readonly OptionButton _resolutionOption;
	private readonly CheckBox _devModeCheck;
	private readonly HSlider _dragSpeedSlider;
	private readonly Label _dragSpeedValueLabel;
	private readonly Button _applyButton;
	private readonly Button _backButton;

	public bool IsOpen { get; private set; }
	public event Action<bool> OpenChanged;
	private float _dragSpeed = DisplaySettingsManager.DefaultDragSpeed;

	public SettingsUiController(Control root)
	{
		_root = root;
		_windowModeOption = root.GetNode<OptionButton>("Panel/WindowModeOption");
		_resolutionOption = root.GetNode<OptionButton>("Panel/ResolutionOption");
		_devModeCheck = root.GetNode<CheckBox>("Panel/DevModeCheck");
		_dragSpeedSlider = root.GetNode<HSlider>("Panel/DragSpeedSlider");
		_dragSpeedValueLabel = root.GetNode<Label>("Panel/DragSpeedValueLabel");
		_applyButton = root.GetNode<Button>("Panel/ApplyButton");
		_backButton = root.GetNode<Button>("Panel/BackButton");

		_windowModeOption.Clear();
		_windowModeOption.AddItem("窗口化", (int)DisplayWindowMode.Windowed);
		_windowModeOption.AddItem("无边框全屏", (int)DisplayWindowMode.BorderlessFullscreen);

		_resolutionOption.Clear();
		foreach (Vector2I resolution in DisplaySettingsManager.OutputResolutions)
			_resolutionOption.AddItem($"{resolution.X} × {resolution.Y}", _resolutionOption.ItemCount);

		_windowModeOption.ItemSelected += OnWindowModeSelected;
		_dragSpeedSlider.MinValue = DisplaySettingsManager.DragSpeedMin;
		_dragSpeedSlider.MaxValue = DisplaySettingsManager.DragSpeedMax;
		_dragSpeedSlider.Step = DisplaySettingsManager.DragSpeedStep;
		_dragSpeedSlider.ValueChanged += OnDragSpeedChanged;
		_applyButton.Pressed += OnApplyPressed;
		_backButton.Pressed += OnBackPressed;

		_root.Visible = false;
	}

	public void Dispose()
	{
		_windowModeOption.ItemSelected -= OnWindowModeSelected;
		_dragSpeedSlider.ValueChanged -= OnDragSpeedChanged;
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
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_windowModeOption.Select(_windowModeOption.GetItemIndex((int)settings.WindowMode));
		_resolutionOption.Select(settings.FindResolutionIndex(settings.OutputResolution));
		_devModeCheck.ButtonPressed = DisplaySettingsManager.DevModeEnabled;
		_dragSpeed = settings.CameraDragSpeed;
		_dragSpeedSlider.Value = _dragSpeed;
		UpdateDragSpeedLabel();
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

	private void OnDragSpeedChanged(double value)
	{
		_dragSpeed = (float)value;
		UpdateDragSpeedLabel();
	}

	private void UpdateDragSpeedLabel()
	{
		_dragSpeedValueLabel.Text = $"{_dragSpeed:0.0}×";
	}

	private void OnApplyPressed()
	{
		DisplayWindowMode mode = (DisplayWindowMode)_windowModeOption.GetSelectedId();
		Vector2I resolution = DisplaySettingsManager.OutputResolutions[_resolutionOption.Selected];
		DisplaySettingsManager.DevModeEnabled = _devModeCheck.ButtonPressed;
		AdapterRegistry.Resolve<DisplaySettingsManager>().SaveAndApply(mode, resolution, _dragSpeed);
	}

	private void OnBackPressed() => Close();
}
