using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class TavernPartyServiceTests
{
	[Fact]
	public void TryAssignTavern_NoTavern_ReturnsFalse()
	{
		Unit unit = CreateAdventurer("ElfRanger", "Elf");
		TavernPartyService service = new();

		bool assigned = service.TryAssignTavern(unit, Taverns());

		Assert.False(assigned);
	}

	[Fact]
	public void TryAssignTavern_NonAdventurer_ReturnsFalse()
	{
		Unit unit = CreateAdventurer("Spearman", "Human");
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();

		bool assigned = service.TryAssignTavern(unit, Taverns(tavern));

		Assert.False(assigned);
	}

	[Fact]
	public void TryAssignTavern_EnemyTavern_ReturnsFalse()
	{
		Unit unit = CreateAdventurer("ElfRanger", "Elf");
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f, true);
		TavernPartyService service = new();

		bool assigned = service.TryAssignTavern(unit, Taverns(tavern));

		Assert.False(assigned);
	}

	[Fact]
	public void FourDistinctRaces_DepartsAndClearsWaiting()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger = CreateAdventurer("ElfRanger", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");
		Unit thief = CreateAdventurer("HalflingThief", "Halfling");
		Unit mage = CreateAdventurer("DragonbornMage", "Dragonborn");

		service.TryAssignTavern(ranger, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.TryAssignTavern(thief, Taverns(tavern));
		service.TryAssignTavern(mage, Taverns(tavern));

		Assert.False(service.IsOccupied(tavern));
		Assert.Null(ranger.AssignedTavern);
		Assert.Null(warrior.AssignedTavern);
		Assert.Null(thief.AssignedTavern);
		Assert.Null(mage.AssignedTavern);
	}

	[Fact]
	public void SameRaceDuplicate_SkipsTavernAndMarchesForward()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger1 = CreateAdventurer("ElfRanger", "Elf");
		Unit ranger2 = CreateAdventurer("ElfSharpshooter", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");
		Unit thief = CreateAdventurer("HalflingThief", "Halfling");
		Unit mage = CreateAdventurer("DragonbornMage", "Dragonborn");

		service.TryAssignTavern(ranger1, Taverns(tavern));
		bool assigned2 = service.TryAssignTavern(ranger2, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.TryAssignTavern(thief, Taverns(tavern));
		Assert.True(service.IsOccupied(tavern));
		Assert.Equal(3, service.GetWaitingUnits(tavern).Count);

		service.TryAssignTavern(mage, Taverns(tavern));

		Assert.False(assigned2);
		Assert.Null(ranger2.AssignedTavern);
		Assert.False(service.IsOccupied(tavern));
		Assert.Null(ranger1.AssignedTavern);
	}

	[Fact]
	public void AdventurerGuild_WaitsAtFourDistinctRaces()
	{
		FakeBuilding tavern = CreateTavern("AdventurerGuild", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger = CreateAdventurer("ElfRanger", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");
		Unit thief = CreateAdventurer("HalflingThief", "Halfling");
		Unit mage = CreateAdventurer("DragonbornMage", "Dragonborn");

		service.TryAssignTavern(ranger, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.TryAssignTavern(thief, Taverns(tavern));
		service.TryAssignTavern(mage, Taverns(tavern));

		Assert.True(service.IsOccupied(tavern));
		Assert.Equal(4, service.GetWaitingUnits(tavern).Count);
	}

	[Fact]
	public void Eject_RemovesOnlyThatAdventurer()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger = CreateAdventurer("ElfRanger", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");

		service.TryAssignTavern(ranger, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.Eject(ranger);

		Assert.Null(ranger.AssignedTavern);
		Assert.Single(service.GetWaitingUnits(tavern));
		Assert.Same(warrior, service.GetWaitingUnits(tavern)[0]);
		Assert.Same(tavern, warrior.AssignedTavern);
	}

	[Fact]
	public void SameRaceCanRejoin_AfterEjectFreesSlot()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger1 = CreateAdventurer("ElfRanger", "Elf");
		Unit ranger2 = CreateAdventurer("ElfSharpshooter", "Elf");

		service.TryAssignTavern(ranger1, Taverns(tavern));
		service.Eject(ranger1);
		bool assigned2 = service.TryAssignTavern(ranger2, Taverns(tavern));

		Assert.True(assigned2);
		Assert.Same(tavern, ranger2.AssignedTavern);
		Assert.Single(service.GetWaitingUnits(tavern));
	}

	[Fact]
	public void RemoveOnDeath_RemovesFromWaitingList()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger = CreateAdventurer("ElfRanger", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");

		service.TryAssignTavern(ranger, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.RemoveOnDeath(ranger);

		Assert.Null(ranger.AssignedTavern);
		Assert.Single(service.GetWaitingUnits(tavern));
	}

	[Fact]
	public void TryAssignTavern_PicksNearestTavern()
	{
		Unit unit = CreateAdventurer("ElfRanger", "Elf");
		unit.SetPosition(0f, 0f);
		FakeBuilding far = CreateTavern("Inn", 5000f, 0f);
		FakeBuilding near = CreateTavern("Inn", 200f, 0f);
		TavernPartyService service = new();

		service.TryAssignTavern(unit, Taverns(far, near));

		Assert.Same(near, unit.AssignedTavern);
		Assert.Same(unit, service.GetWaitingUnits(near)[0]);
		Assert.Empty(service.GetWaitingUnits(far));
	}

	[Fact]
	public void ReleaseTavern_ReleasesAllWaitingUnits()
	{
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		TavernPartyService service = new();
		Unit ranger = CreateAdventurer("ElfRanger", "Elf");
		Unit warrior = CreateAdventurer("DwarfWarrior", "Dwarf");

		service.TryAssignTavern(ranger, Taverns(tavern));
		service.TryAssignTavern(warrior, Taverns(tavern));
		service.ReleaseTavern(tavern);

		Assert.False(service.IsOccupied(tavern));
		Assert.Null(ranger.AssignedTavern);
		Assert.Null(warrior.AssignedTavern);
	}

	[Fact]
	public void UpdateTargeting_AssignedNearTavern_EntersWaiting()
	{
		Unit unit = CreateAdventurer("ElfRanger", "Elf");
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		unit.SetPosition(120f, 100f);
		unit.AssignedTavern = tavern;

		unit.UpdateTargeting(null);

		Assert.Equal(UnitState.Waiting, unit.State);
	}

	[Fact]
	public void UpdateTargeting_AssignedFarFromTavern_Marches()
	{
		Unit unit = CreateAdventurer("ElfRanger", "Elf");
		FakeBuilding tavern = CreateTavern("Inn", 0f, 0f);
		unit.SetPosition(1000f, 0f);
		unit.AssignedTavern = tavern;

		unit.UpdateTargeting(null);

		Assert.Equal(UnitState.Marching, unit.State);
	}

	[Fact]
	public void UpdateBehavior_Waiting_StandsStill()
	{
		RecordingNavPort navPort = new();
		Unit unit = CreateAdventurer("ElfRanger", "Elf", navPort);
		unit.State = UnitState.Waiting;

		unit.UpdateBehavior(0.5f, 999f, 999f);

		Assert.Equal(unit.GameX, navPort.LastTargetX);
		Assert.Equal(unit.GameY, navPort.LastTargetY);
	}

	private static Unit CreateAdventurer(string typeId, string race, INavigationPort navPort = null)
	{
		UnitStats stats = new()
		{
			TypeId = typeId,
			Size = UnitSize.Small,
			AttackType = AttackType.Melee,
			DamageType = DamageType.Slash,
			ArmorType = ArmorType.Leather,
			Health = 100,
			Damage = 10,
			Speed = 100,
			AttackRange = 45f,
			AttackCooldown = 1f,
			VisionRange = 300,
			UnitCost = 1,
			Race = race,
		};
		Unit unit = new(navPort ?? new RecordingNavPort());
		unit.Initialize(stats, true);
		unit.SetDamageMatrix(new DamageMatrix());
		return unit;
	}

	private static FakeBuilding CreateTavern(string typeId, float minX, float minY, bool enemyOfUnit = false)
	{
		return new FakeBuilding
		{
			TypeId = typeId,
			IsEnemyOfUnit = enemyOfUnit,
			MinX = minX,
			MinY = minY,
			MaxX = minX + 200f,
			MaxY = minY + 200f,
		};
	}

	private static IReadOnlyList<IBuildingRef> Taverns(params IBuildingRef[] taverns)
	{
		return new List<IBuildingRef>(taverns);
	}
}