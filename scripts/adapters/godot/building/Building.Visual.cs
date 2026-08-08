// 建筑表现：贴图、工作进度、受损与状态图标
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Building : Area2D
{
	private void TryApplyVisual()
	{
		Sprite2D sprite = _sprite ?? GetNodeOrNull<Sprite2D>("View/Sprite");
		if (sprite == null)
			return;

		if (!_visualApplied)
		{
			BuildingSys.ApplyVisual(this);
			_visualApplied = true;
		}

		_sprite = sprite;
		_spriteBasePosition = sprite.Position;
		if (_originalMaterial == null)
			_originalMaterial = sprite.Material;
	}

	private void PlayDestructionEffect()
	{
		Node parent = GetParent();
		if (parent == null)
			return;

		Image img = Image.CreateEmpty(8, 8, false, Image.Format.Rgba8);
		for (int x = 0; x < 8; x++)
			for (int y = 0; y < 8; y++)
				img.SetPixel(x, y, new Color(1, 1, 1, 1));
		ImageTexture tex = ImageTexture.CreateFromImage(img);

		ParticleProcessMaterial mat = new ParticleProcessMaterial();
		mat.Gravity = new Vector3(0, 200, 0);
		mat.InitialVelocityMin = -80;
		mat.InitialVelocityMax = 80;
		mat.ScaleMin = 1.5f;
		mat.ScaleMax = 3.5f;
		mat.DampingMin = 2f;
		mat.DampingMax = 5f;
		mat.AngleMin = 0;
		mat.AngleMax = 360;
		mat.AngularVelocityMin = -360;
		mat.AngularVelocityMax = 360;
		mat.Color = new Color(0.5f, 0.45f, 0.4f, 1f);

		GpuParticles2D particles = new GpuParticles2D();
		particles.OneShot = true;
		particles.Explosiveness = 1f;
		particles.Amount = 16;
		particles.Lifetime = 0.6f;
		particles.ProcessMaterial = mat;
		particles.Texture = tex;
		particles.GlobalPosition = GlobalPosition;
		particles.Emitting = true;
		parent.AddChild(particles);

		if (_sprite != null)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(_sprite, "modulate:a", 0f, 0.3f);
		}

		Tween delayTween = CreateTween();
		delayTween.TweenInterval(0.7f);
		delayTween.TweenCallback(Callable.From(() =>
		{
			particles.QueueFree();
			QueueFree();
		}));
	}

	private void UpdateWorkEffectFromProgress()
	{
		if (_sprite == null || _workRequired <= 0f)
			return;

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		float progress = Mathf.Clamp(_workDone / _workRequired, 0f, 1f);
		_workMaterial.SetShaderParameter("fill_amount", progress);
	}

	private void PlayWorkJumpVisual()
	{
		if (_sprite == null)
			return;

		CancelJumpTween();
		_jumpTweenAwaitingResume = false;

		const float jumpOffset = -6f;
		const float jumpUpDuration = 0.08f;
		const float jumpDownDuration = 0.1f;

		_jumpTween = CreateTween();
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y + jumpOffset, jumpUpDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_jumpTween.TweenProperty(_sprite, "position:y", _spriteBasePosition.Y, jumpDownDuration)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
	}

	private void EnsureWorkMaterial()
	{
		if (_workMaterial != null)
			return;

		Shader shader = GD.Load<Shader>("res://assets/shaders/building_work.gdshader");
		_workMaterial = new ShaderMaterial { Shader = shader };
		_workMaterial.SetShaderParameter("brighten", 0.35f);
	}

	private void CancelJumpTween()
	{
		if (_jumpTween != null && _jumpTween.IsValid())
			_jumpTween.Kill();
		_jumpTween = null;
	}

	private void ResetWorkVisual()
	{
		if (_sprite == null)
			return;
		_sprite.Material = _originalMaterial;
		_sprite.Position = _spriteBasePosition;
		UpdateDamageVisual();
	}

	private void UpdateDamageVisual()
	{
		if (_sprite == null)
			return;

		if (Health <= 0)
			_sprite.Modulate = new Color(0.55f, 0.55f, 0.6f, 0.75f);
		else if (IsDamaged)
			_sprite.Modulate = new Color(0.85f, 0.75f, 0.75f);
		else
			_sprite.Modulate = BuildingSys.GetSpriteModulate(TypeId);
	}

	private void RefreshOperationalState()
	{
		SyncStateIconPosition();
		UpdateStateIcon();

		if (!IsOperational)
		{
			if (IsManuallyPaused)
			{
				if (_workActive && !_workPaused)
					PauseWork();
			}
			else
			{
				StopWork();
			}
		}
		else
		{
			UpdateWorkCycle();
		}

		Castle castle = CastleRef;
		if (castle != null)
			_adjacencyService.RefreshCastle(castle.GetBuildingStates());
	}

	private void SyncStateIconPosition()
	{
		if (_stateIcon == null || CastleRef == null)
			return;

		Vector2I mainOffset = BuildingSys.GetMainCellOffset(TypeId);
		Vector2 mainCenter = CastleRef.GetCellCenter(GridX + mainOffset.X, GridY + mainOffset.Y);
		_stateIcon.Position = mainCenter - Position;
		if (_hordeIndicator != null)
			_hordeIndicator.Position = mainCenter - Position;
	}

	private void UpdateHordeIndicator()
	{
		if (_hordeIndicator == null || CastleRef == null)
			return;

		List<IBuildingState> all = CastleRef.GetBuildingStates();
		List<IAdjacencyBuilding> domain = all.OfType<IAdjacencyBuilding>().ToList();
		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(this, domain, GameManager.Get<IBuildingRepository>());
		_hordeIndicator.SetAdjacencyCellCount(count);
	}

	private void UpdateStateIcon()
	{
		if (_stateIcon == null || BuildingSys.IsCoreBuilding(TypeId))
		{
			_stateIcon?.SetPaused(false);
			_stateIcon?.SetCombineProhibited(false);
			return;
		}

		if (IsManuallyPaused)
			_stateIcon.SetPaused(true);
		else
			_stateIcon.SetPaused(false);

		_stateIcon.SetCombineProhibited(IsCombineProhibited);
	}

}
