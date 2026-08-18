// 建筑视图：贴图、工作材质、状态图标与部落邻接指示
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;
using System.Linq;

public sealed class BuildingView
{
	private readonly Building _owner;

	private readonly BuildingSystem _buildingSystem;

	private Sprite2D _sprite;

	private Vector2 _spriteBasePosition;

	private Material _originalMaterial;

	private ShaderMaterial _workMaterial;

	private bool _visualApplied;

	private BuildingStateIcon _stateIcon;

	private HordeAdjacencyIndicator _hordeIndicator;

	private const float WorkBrightenAmount = 0.35f;

	public BuildingView(Building owner, BuildingSystem buildingSystem)
	{
		_owner = owner;
		_buildingSystem = buildingSystem;
	}

	public void Initialize()
	{
		_sprite = _owner.GetNodeOrNull<Sprite2D>("View/Sprite");
		TryApplyVisual();
		_stateIcon = new BuildingStateIcon();
		Node2D effects = _owner.GetNodeOrNull<Node2D>("Effects");
		effects?.AddChild(_stateIcon);
		if (_buildingSystem.GetFactionId(_owner.TypeId) == FactionDef.HordeId)
		{
			_hordeIndicator = new HordeAdjacencyIndicator();
			effects?.AddChild(_hordeIndicator);
		}
		SyncStateIconPosition();
		UpdateStateIcon();
	}

	public void TryApplyVisual()
	{
		if (_sprite == null)
			_sprite = _owner.GetNodeOrNull<Sprite2D>("View/Sprite");
		if (_sprite == null)
			return;

		if (!_visualApplied)
		{
			_buildingSystem.ApplyVisual(_owner);
			_visualApplied = true;
		}

		_spriteBasePosition = _sprite.Position;
		if (_originalMaterial == null)
			_originalMaterial = _sprite.Material;
	}

	public void UpdateWorkEffectFromProgress(bool hasWork, float progress)
	{
		if (_sprite == null || !hasWork)
			return;

		EnsureWorkMaterial();
		_sprite.Material = _workMaterial;
		_workMaterial.SetShaderParameter("fill_amount", progress);
	}

	public void ResetWorkVisual()
	{
		if (_sprite == null)
			return;
		_sprite.Material = _originalMaterial;
		_sprite.Position = _spriteBasePosition;
	}

	public void SyncStateIconPosition()
	{
		if (_stateIcon == null || _owner.GetCastle() == null)
			return;

		Vector2I mainOffset = _buildingSystem.GetMainCellOffset(_owner.TypeId);
		Vector2 mainCenter = _owner.GetCastle().GetCellCenter(_owner.AnchorGridX + mainOffset.X, _owner.AnchorGridY + mainOffset.Y);
		_stateIcon.Position = mainCenter - _owner.Position;
		if (_hordeIndicator != null)
			_hordeIndicator.Position = mainCenter - _owner.Position;
	}

	public void UpdateHordeIndicator()
	{
		if (_hordeIndicator == null || _owner.GetCastle() == null)
			return;

		List<IBuildingState> all = _owner.GetCastle().GetBuildingStates();
		List<IAdjacencyBuilding> domain = all.OfType<IAdjacencyBuilding>().ToList();
		int count = HordeAdjacencyRules.CountAdjacencyBonusCells(_owner, domain, GameManager.Get<IBuildingRepository>());
		_hordeIndicator.SetAdjacencyCellCount(count);
	}

	public void UpdateStateIcon()
	{
		if (_stateIcon == null || _buildingSystem.IsCoreBuilding(_owner.TypeId))
		{
			_stateIcon?.SetPaused(false);
			_stateIcon?.SetCombineProhibited(false);
			return;
		}

		if (_owner.IsManuallyPaused)
			_stateIcon.SetPaused(true);
		else
			_stateIcon.SetPaused(false);

		_stateIcon.SetCombineProhibited(_owner.IsCombineProhibited);
	}

	private void EnsureWorkMaterial()
	{
		if (_workMaterial != null)
			return;

		Shader shader = GD.Load<Shader>("res://assets/shaders/building_work.gdshader");
		_workMaterial = new ShaderMaterial { Shader = shader };
		_workMaterial.SetShaderParameter("brighten", WorkBrightenAmount);
	}
}
