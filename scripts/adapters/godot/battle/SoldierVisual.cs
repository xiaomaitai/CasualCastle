using Godot;

public class SoldierVisual
{
    private UnitCardView _card;
    private Color _portraitTint = Colors.White;
    private float _hitFlashTimer;
    private bool _isSleeping;

    public bool IsFlashing => _hitFlashTimer > 0f;

    public void Initialize(Node2D body)
    {
        _card = body.GetNode<UnitCardView>("View/UnitCard");
    }

    public void ApplyStats(string typeId, bool isPlayerUnit, float displaySize)
    {
        _portraitTint = Colors.White;
        _card.Configure(typeId, isPlayerUnit, displaySize);
    }

    public void StartHitFlash()
    {
        _hitFlashTimer = 0.1f;
        _card.SetPortraitTint(Colors.White);
        _card.SetStatus(UnitCardStatus.Hit);
    }

    public void UpdateHitFlash(float dt)
    {
        if (_hitFlashTimer > 0f)
        {
            _hitFlashTimer -= dt;
            if (_hitFlashTimer <= 0f)
            {
                _card.SetPortraitTint(_portraitTint);
                _card.SetStatus(_isSleeping ? UnitCardStatus.Sleeping : UnitCardStatus.None);
            }
        }
    }

    public void UpdateSleepVisual(bool isActive, bool isSleeping)
    {
        _isSleeping = isSleeping;
        _card.SetDimmed(!isActive);
        if (_hitFlashTimer <= 0f)
            _card.SetStatus(isSleeping ? UnitCardStatus.Sleeping : UnitCardStatus.None);
    }

    public void SetBaseModulate(Color color)
    {
        _portraitTint = color;
        if (_hitFlashTimer <= 0f)
            _card.SetPortraitTint(color);
    }

    public void SetHealth(int current, int maximum)
    {
        _card.SetHealth(current, maximum);
    }

    public void SetSelected(bool selected)
    {
        _card.SetSelected(selected);
    }
}
