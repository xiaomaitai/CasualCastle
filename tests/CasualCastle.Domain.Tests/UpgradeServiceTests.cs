using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Moq;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class UpgradeServiceTests
{
	private static UpgradeRules CreateRules()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "OrcCamp", ResultTypeId = "OrcFortress" }
		});
		return rules;
	}

	private static Mock<IBuildingRepository> CreateRepo(int sourceValue, int resultValue)
	{
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("OrcCamp")).Returns(sourceValue);
		repo.Setup(r => r.GetValue("OrcFortress")).Returns(resultValue);
		repo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(false);
		repo.Setup(r => r.GetFactionId(It.IsAny<string>())).Returns("alliance");
		repo.Setup(r => r.GetCombineTier("OrcFortress")).Returns(1);
		return repo;
	}

	private static Mock<IBuildingState> CreateSource()
	{
		Mock<IBuildingState> source = new();
		source.SetupGet(s => s.TypeId).Returns("OrcCamp");
		source.SetupGet(s => s.AnchorGridX).Returns(4);
		source.SetupGet(s => s.AnchorGridY).Returns(6);
		source.SetupGet(s => s.IsDestroyed).Returns(false);
		source.SetupGet(s => s.IsPlayerOwned).Returns(true);
		return source;
	}

	[Fact]
	public void TryUpgrade_ValidUpgrade_SpendsValueDifferenceAndPreservesState()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource();
		Mock<IBuildingState> result = new();
		Mock<IUpgradeBuildingFactory> factory = new();
		factory.Setup(f => f.Create("OrcFortress", 4, 6, 0.5f)).Returns(result.Object);
		UpgradeService service = new(rules, repo.Object);
		int spent = 0;

		IBuildingState upgraded = service.TryUpgrade(source.Object, "OrcFortress", 0.5f, cost => { spent = cost; return true; }, factory.Object);

		Assert.Same(result.Object, upgraded);
		Assert.Equal(4, spent);
		factory.Verify(f => f.Destroy(source.Object), Times.Once);
	}

	[Fact]
	public void TryUpgrade_NoRecipe_ReturnsNullAndSpendsNothing()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource();
		source.SetupGet(s => s.TypeId).Returns("MageTower");
		Mock<IUpgradeBuildingFactory> factory = new();
		UpgradeService service = new(rules, repo.Object);
		bool spentCalled = false;

		IBuildingState upgraded = service.TryUpgrade(source.Object, "OrcFortress", 0.5f, cost => { spentCalled = true; return true; }, factory.Object);

		Assert.Null(upgraded);
		Assert.False(spentCalled);
	}

	[Fact]
	public void TryUpgrade_CannotAfford_ReturnsNullAndKeepsSource()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource();
		Mock<IUpgradeBuildingFactory> factory = new();
		UpgradeService service = new(rules, repo.Object);

		IBuildingState upgraded = service.TryUpgrade(source.Object, "OrcFortress", 0.5f, cost => false, factory.Object);

		Assert.Null(upgraded);
		factory.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<float>()), Times.Never);
		factory.Verify(f => f.Destroy(It.IsAny<IBuildingState>()), Times.Never);
	}

	[Fact]
	public void TryUpgrade_ResultValueLowerThanSource_ReturnsNull()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(7, 3);
		Mock<IBuildingState> source = CreateSource();
		Mock<IUpgradeBuildingFactory> factory = new();
		UpgradeService service = new(rules, repo.Object);
		bool spentCalled = false;

		IBuildingState upgraded = service.TryUpgrade(source.Object, "OrcFortress", 0.5f, cost => { spentCalled = true; return true; }, factory.Object);

		Assert.Null(upgraded);
		Assert.False(spentCalled);
	}

	[Fact]
	public void TryUpgrade_AlwaysAvailable_IgnoresCombinePhase()
	{
		UpgradeRules rules = CreateRules();
		Mock<IBuildingRepository> repo = CreateRepo(3, 7);
		Mock<IBuildingState> source = CreateSource();
		Mock<IBuildingState> result = new();
		Mock<IUpgradeBuildingFactory> factory = new();
		factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<float>())).Returns(result.Object);
		UpgradeService service = new(rules, repo.Object);

		IBuildingState upgraded = service.TryUpgrade(source.Object, "OrcFortress", 0.5f, cost => true, factory.Object);

		Assert.Same(result.Object, upgraded);
	}

	[Fact]
	public void TryUpgrade_HordeCoreWithoutBurrows_ReturnsNull()
	{
		UpgradeRules rules = new();
		rules.LoadRecipes(new List<UpgradeRecipe>
		{
			new() { SourceTypeId = "HordeCoreL1", ResultTypeId = "HordeCoreL2" }
		});
		Mock<IBuildingRepository> repo = new();
		repo.Setup(r => r.GetValue("HordeCoreL1")).Returns(0);
		repo.Setup(r => r.GetValue("HordeCoreL2")).Returns(20);
		Mock<IBuildingState> source = CreateSource();
		source.SetupGet(s => s.TypeId).Returns("HordeCoreL1");
		Mock<IUpgradeBuildingFactory> factory = new();
		UpgradeService service = new(rules, repo.Object);

		IBuildingState upgraded = service.TryUpgrade(source.Object, "HordeCoreL2", 0.5f, cost => true, factory.Object, int.MaxValue, 0);

		Assert.Null(upgraded);
		factory.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<float>()), Times.Never);
	}
}
