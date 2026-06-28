namespace CasualCastle.Domain.Shared;

public readonly struct GameVector2
{
	public int X { get; }
	public int Y { get; }

	public GameVector2(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static GameVector2 operator +(GameVector2 a, GameVector2 b) => new(a.X + b.X, a.Y + b.Y);
	public static GameVector2 operator -(GameVector2 a, GameVector2 b) => new(a.X - b.X, a.Y - b.Y);
}
