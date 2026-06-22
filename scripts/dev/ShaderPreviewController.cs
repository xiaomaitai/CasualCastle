using Godot;

public partial class ShaderPreviewController : Control
{
    private const string ScratchPath = "user://shader_preview_scratch.gdshader";
    private const string DefaultPath = "res://assets/dev/shader_preview_default.gdshader";

    private CodeEdit _editor;
    private ColorRect _preview;
    private Label _statusLabel;
    private ShaderMaterial _material;

    public override void _Ready()
    {
        _editor = GetNode<CodeEdit>("HSplit/EditorColumn/CodeEdit");
        _preview = GetNode<ColorRect>("HSplit/PreviewColumn/PreviewFrame/Preview");
        _statusLabel = GetNode<Label>("HSplit/EditorColumn/Toolbar/StatusLabel");

        GetNode<Button>("HSplit/EditorColumn/Toolbar/CompileButton").Pressed += CompileShader;
        GetNode<Button>("HSplit/EditorColumn/Toolbar/ResetButton").Pressed += ResetToDefault;

        _editor.TextChanged += OnEditorTextChanged;
        LoadInitialShader();
        CompileShader();
    }

    public override void _Process(double _)
    {
        if (_material == null)
            return;

        _material.SetShaderParameter("time", (float)Time.GetTicksMsec() / 1000.0);
        _material.SetShaderParameter("resolution", _preview.Size);
        _material.SetShaderParameter("mouse", _preview.GetLocalMousePosition());
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
            return;

        if (keyEvent.Keycode != Key.F5)
            return;

        CompileShader();
        GetViewport().SetInputAsHandled();
    }

    private void OnEditorTextChanged()
    {
        _statusLabel.Text = "已修改（F5 编译）";
    }

    private void LoadInitialShader()
    {
        if (FileAccess.FileExists(ScratchPath))
        {
            using FileAccess file = FileAccess.Open(ScratchPath, FileAccess.ModeFlags.Read);
            _editor.Text = file.GetAsText();
            return;
        }

        _editor.Text = FileAccess.GetFileAsString(DefaultPath);
    }

    private void ResetToDefault()
    {
        _editor.Text = FileAccess.GetFileAsString(DefaultPath);
        CompileShader();
    }

    private void CompileShader()
    {
        string source = _editor.Text;
        using (FileAccess file = FileAccess.Open(ScratchPath, FileAccess.ModeFlags.Write))
            file.StoreString(source);

        Shader shader = ResourceLoader.Load<Shader>(ScratchPath, null, ResourceLoader.CacheMode.Ignore);
        if (shader == null)
        {
            _statusLabel.Text = "编译失败";
            return;
        }

        _material ??= new ShaderMaterial();
        _material.Shader = shader;
        _preview.Material = _material;
        _statusLabel.Text = "编译成功";
    }
}
