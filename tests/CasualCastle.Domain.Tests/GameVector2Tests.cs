using CasualCastle.Domain.Shared;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class GameVector2Tests
{
	[Fact]
	public void Constructor_SetsXAndY()
	{
		GameVector2 v = new(10, 20);
		Assert.Equal(10, v.X);
		Assert.Equal(20, v.Y);
	}

	[Fact]
	public void Addition_ReturnsSum()
	{
		GameVector2 a = new(10, 20);
		GameVector2 b = new(5, 7);
		GameVector2 result = a + b;

		Assert.Equal(15, result.X);
		Assert.Equal(27, result.Y);
	}

	[Fact]
	public void Subtraction_ReturnsDifference()
	{
		GameVector2 a = new(10, 20);
		GameVector2 b = new(5, 7);
		GameVector2 result = a - b;

		Assert.Equal(5, result.X);
		Assert.Equal(13, result.Y);
	}
}
