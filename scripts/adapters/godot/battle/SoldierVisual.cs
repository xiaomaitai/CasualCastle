using Godot;
using System;

public class SoldierVisual
{
    private Sprite2D _sprite;
    private SoldierSleepZEffect _sleepZEffect;
    private Color _baseModulate = Colors.White;
    private float _hitFlashTimer;

    public bool IsFlashing => _hitFlashTimer > 0f;

    public void Initialize(Node2D body)
    {
        _sprite = body?.GetNodeOrNull<Sprite2D>("View/Sprite");
        _sleepZEffect = body?.GetNodeOrNull<SoldierSleepZEffect>("Effects/SleepZEffect");
    }

    public void ApplyStats(uint unitColor, float displaySize)
    {
        _baseModulate = new Color(
            ((unitColor >> 16) & 0xFF) / 255f,
            ((unitColor >> 8) & 0xFF) / 255f,
            (unitColor & 0xFF) / 255f);

        if (_sprite != null)
        {
            Texture2D texture = _sprite.Texture;
            if (texture != null)
            {
                float scale = displaySize / Math.Max(texture.GetWidth(), texture.GetHeight());
                _sprite.Scale = new Vector2(scale, scale);
            }
            _sprite.Position = new Vector2(0, -displaySize * 0.5f);
        }
    }

    public void StartHitFlash()
    {
        _hitFlashTimer = 0.1f;
        if (_sprite != null)
            _sprite.Modulate = Colors.White;
    }

    public void UpdateHitFlash(float dt)
    {
        if (_hitFlashTimer > 0f)
        {
            _hitFlashTimer -= dt;
            if (_hitFlashTimer <= 0f && _sprite != null)
                _sprite.Modulate = _baseModulate;
        }
    }

    public void UpdateSleepVisual(bool isActive, bool isSleeping)
    {
        if (_sprite != null && _hitFlashTimer <= 0f)
        {
            _sprite.Modulate = isActive
                ? _baseModulate
                : new Color(_baseModulate.R * 0.75f, _baseModulate.G * 0.8f, _baseModulate.B, 0.85f);
        }

        _sleepZEffect?.SetSleeping(isSleeping);
    }

    public void SetBaseModulate(Color color)
    {
        _baseModulate = color;
    }
}
