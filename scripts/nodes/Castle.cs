using Godot;
using System;

public partial class Castle : Area2D
{
	[Export]
	public bool IsPlayerCastle;

	[Export]
	public int MaxHealth = 100;

	[Export]
	public int GridColumns = 8;

	[Export]
	public int GridRows = 8;

	[Export]
	public int CellSize = 64;

	[Export]
	public int BlockSize = 60;

	[Export]
	public Color BlockColor = new Color(1, 1, 1, 0.06f);

	[Export]
	public Color AreaBgColor = new Color(1, 1, 1, 0.03f);

	public int Health { get; private set; }

	private ProgressBar _healthBar;

	public override void _Ready()
	{
		Health = MaxHealth;
		_healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
		UpdateHealthBar();
	}

	public void TakeDamage(int amount)
	{
		Health = Math.Max(0, Health - amount);
		UpdateHealthBar();
		GameManager.Instance?.TakeDamage(IsPlayerCastle, amount);
	}

	public void ResetHealth()
	{
		Health = MaxHealth;
		UpdateHealthBar();
	}

	private void UpdateHealthBar()
	{
		if (_healthBar == null) return;
		_healthBar.MaxValue = MaxHealth;
		_healthBar.Value = Health;
	}

	public override void _Draw()
	{
		int totalWidth = GridColumns * CellSize;
		int totalHeight = GridRows * CellSize;

		DrawRect(new Rect2(0, 0, totalWidth, totalHeight), AreaBgColor);

		int offset = (CellSize - BlockSize) / 2;

		for (int row = 0; row < GridRows; row++)
		{
			for (int col = 0; col < GridColumns; col++)
			{
				Vector2 position = new Vector2(col * CellSize + offset, row * CellSize + offset);
				DrawRect(new Rect2(position, new Vector2(BlockSize, BlockSize)), BlockColor);
			}
		}
	}
}
