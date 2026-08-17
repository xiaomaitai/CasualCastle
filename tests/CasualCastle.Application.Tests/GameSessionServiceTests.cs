using CasualCastle.Application;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Application.Tests;

public class GameSessionServiceTests
{
	private static GameSessionService CreateService(RecordingSaveRepository repo, RecordingQuery query, RecordingCommand command, RecordingShop shop, IReadOnlyList<CardData> catalog, Mock<IBuildingRepository> buildingRepo)
	{
		GameSessionPorts ports = TestPorts.Create(query, command, shop, new RecordingCapture());
		return new GameSessionService(repo, catalog, buildingRepo.Object, ports);
	}

	[Fact]
	public void SaveGame_CollectsEveryField()
	{
		RecordingSaveRepository repo = new();
		RecordingQuery query = new()
		{
			FactionId = FactionIds.Dungeon,
			Gold = 456,
			CurrentNightIndex = 3,
			PendingReplayReportId = "report_1",
			RitualStudyCompletionCount = 2,
			LearnedRitualIds = new List<string> { "ritual_a" },
			DungeonKingXp = 789,
			DungeonLearnedSkillIds = new List<string> { "skill_a", "skill_b" },
			DungeonLearnedSkillLevels = new List<int> { 1, 3 },
			PlayerBuildingStates = new List<IBuildingState> { new FakeBuildingState("barracks", 2, 5) },
			HandCards = new List<CardData>
			{
				new() { Id = "tome", Name = "魔法书", Cost = 5, Weight = 2, ItemType = "dark_book", Attributes = new Dictionary<string, string> { { "ritual", "ritual_fire" } } },
			},
		};
		GameSessionService service = CreateService(repo, query, new RecordingCommand(), new RecordingShop(), new List<CardData>(), new Mock<IBuildingRepository>());

		service.SaveGame(0);

		Assert.Equal(1, repo.SaveCalls);
		SaveData data = repo.LastSaved;
		Assert.Equal(0, data.SlotIndex);
		Assert.Equal(FactionIds.Dungeon, data.FactionId);
		Assert.Equal(456, data.Gold);
		Assert.Equal(3, data.CurrentNightIndex);
		Assert.Equal("report_1", data.PendingReplayReportId);
		Assert.Equal(2, data.RitualStudyCompletionCount);
		Assert.Equal(new List<string> { "ritual_a" }, data.LearnedRitualIds);
		Assert.Equal(789, data.DungeonKingXp);
		Assert.Equal(new List<string> { "skill_a", "skill_b" }, data.DungeonLearnedSkillIds);
		Assert.Equal(new List<int> { 1, 3 }, data.DungeonLearnedSkillLevels);
		BuildingSaveEntry building = Assert.Single(data.Buildings);
		Assert.Equal("barracks", building.TypeId);
		Assert.Equal(2, building.AnchorGridX);
		Assert.Equal(5, building.AnchorGridY);
		Assert.Equal(100, building.Health);
		CardSaveEntry card = Assert.Single(data.HandCards);
		Assert.Equal("tome", card.Id);
		Assert.False(card.IsPlaceable);
		Assert.Equal("dark_book", card.ItemType);
		Assert.Equal("ritual_fire", card.Attributes["ritual"]);
	}

	[Fact]
	public void RestoreSession_RoutesNonCoreAndCoreBuildings()
	{
		Mock<IBuildingRepository> buildingRepo = new();
		buildingRepo.Setup(r => r.IsCoreBuilding("barracks")).Returns(false);
		buildingRepo.Setup(r => r.IsCoreBuilding("altar")).Returns(true);
		RecordingCommand command = new();
		SaveData data = new()
		{
			FactionId = FactionIds.Alliance,
			Gold = 100,
			Buildings = new List<BuildingSaveEntry>
			{
				new() { TypeId = BuildingTypeIds.CastleHeart, AnchorGridX = 0, AnchorGridY = 0, Health = 100 },
				new() { TypeId = "barracks", AnchorGridX = 2, AnchorGridY = 5, Health = 80 },
				new() { TypeId = "altar", AnchorGridX = 1, AnchorGridY = 1, Health = 40 },
			},
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), new List<CardData>(), buildingRepo);

		service.RestoreSession(data);

		(string TypeId, int X, int Y, int Health) placed = Assert.Single(command.Placed);
		Assert.Equal("barracks", placed.TypeId);
		Assert.Equal(2, placed.X);
		Assert.Equal(5, placed.Y);
		Assert.Equal(80, placed.Health);
		(string TypeId, int Health) core = Assert.Single(command.CoreCalls);
		Assert.Equal("altar", core.TypeId);
		Assert.Equal(40, core.Health);
	}

	[Fact]
	public void RestoreSession_UnreplacedCore_KeepsIterating()
	{
		Mock<IBuildingRepository> buildingRepo = new();
		buildingRepo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(true);
		RecordingCommand command = new() { CoreResult = false };
		SaveData data = new()
		{
			Buildings = new List<BuildingSaveEntry>
			{
				new() { TypeId = "core_a", AnchorGridX = 0, AnchorGridY = 0, Health = 50 },
				new() { TypeId = "core_b", AnchorGridX = 0, AnchorGridY = 0, Health = 60 },
			},
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), new List<CardData>(), buildingRepo);

		service.RestoreSession(data);

		Assert.Equal(2, command.CoreCalls.Count);
	}

	[Fact]
	public void RestoreSession_ReplacedCore_StopsIterating()
	{
		Mock<IBuildingRepository> buildingRepo = new();
		buildingRepo.Setup(r => r.IsCoreBuilding(It.IsAny<string>())).Returns(true);
		RecordingCommand command = new() { CoreResult = true };
		SaveData data = new()
		{
			Buildings = new List<BuildingSaveEntry>
			{
				new() { TypeId = "core_a", AnchorGridX = 0, AnchorGridY = 0, Health = 50 },
				new() { TypeId = "core_b", AnchorGridX = 0, AnchorGridY = 0, Health = 60 },
			},
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), new List<CardData>(), buildingRepo);

		service.RestoreSession(data);

		Assert.Single(command.CoreCalls);
	}

	[Fact]
	public void RestoreSession_RestoresEconomyHandFactionAndServices()
	{
		RecordingCommand command = new();
		RecordingShop shop = new();
		SaveData data = new()
		{
			FactionId = FactionIds.Dungeon,
			Gold = 250,
			CurrentNightIndex = 2,
			PendingReplayReportId = "report_9",
			RitualStudyCompletionCount = 3,
			LearnedRitualIds = new List<string> { "ritual_x" },
			DungeonKingXp = 77,
			DungeonLearnedSkillIds = new List<string> { "skill_k" },
			DungeonLearnedSkillLevels = new List<int> { 2 },
			HandCards = new List<CardSaveEntry> { new() { Id = "unit_card", Name = "弓手", Cost = 3, BuildingType = "barracks", Weight = 1, IsPlaceable = true } },
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, shop, new List<CardData>(), new Mock<IBuildingRepository>());

		service.RestoreSession(data);

		Assert.Equal(250, command.Gold);
		Assert.Equal(1, command.ResetHandCalls);
		CardData added = Assert.Single(command.AddedCards);
		Assert.Equal("unit_card", added.Id);
		Assert.True(added.IsPlaceable);
		Assert.Equal(2, command.NightIndex);
		Assert.Equal("report_9", command.ReplayReportId);
		Assert.Equal(3, command.RitualCount);
		Assert.Equal(new List<string> { "ritual_x" }, command.RitualIds);
		Assert.Equal(77, command.KingXp);
		Assert.Equal(new List<string> { "skill_k" }, command.SkillIds);
		Assert.Equal(new List<int> { 2 }, command.SkillLevels);
		Assert.Equal(FactionIds.Dungeon, shop.Faction);
		Assert.Equal(1, command.RefreshPlayerCalls);
	}

	[Fact]
	public void RestoreSession_EmptyReplayReport_DoesNotSet()
	{
		RecordingCommand command = new();
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), new List<CardData>(), new Mock<IBuildingRepository>());

		service.RestoreSession(new SaveData { PendingReplayReportId = "" });

		Assert.Null(command.ReplayReportId);
	}

	[Fact]
	public void RestoreSession_LegacyItemCard_BackfillsFromCatalog()
	{
		RecordingCommand command = new();
		List<CardData> catalog = new()
		{
			new CardData { Id = "tome", ItemType = "dark_book", Attributes = new Dictionary<string, string> { { "ritual", "ritual_fire" } } },
		};
		SaveData data = new()
		{
			HandCards = new List<CardSaveEntry> { new() { Id = "tome", Name = "魔法书", Cost = 5, Weight = 2, IsPlaceable = false, ItemType = "" } },
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), catalog, new Mock<IBuildingRepository>());

		service.RestoreSession(data);

		CardData added = Assert.Single(command.AddedCards);
		Assert.Equal("dark_book", added.ItemType);
		Assert.Equal("ritual_fire", added.Attributes["ritual"]);
	}

	[Fact]
	public void RestoreSession_PlaceableCard_SkipsCatalogBackfill()
	{
		RecordingCommand command = new();
		List<CardData> catalog = new()
		{
			new CardData { Id = "barracks_card", ItemType = "dark_book", Attributes = new Dictionary<string, string> { { "ritual", "x" } } },
		};
		SaveData data = new()
		{
			HandCards = new List<CardSaveEntry> { new() { Id = "barracks_card", BuildingType = "barracks", IsPlaceable = true, ItemType = "" } },
		};
		GameSessionService service = CreateService(new RecordingSaveRepository(), new RecordingQuery(), command, new RecordingShop(), catalog, new Mock<IBuildingRepository>());

		service.RestoreSession(data);

		CardData added = Assert.Single(command.AddedCards);
		Assert.Equal("", added.ItemType);
		Assert.Empty(added.Attributes);
	}
}
