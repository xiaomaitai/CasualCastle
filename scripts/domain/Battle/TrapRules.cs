// 奸奇陷阱规则：陷阱类型判定与触发配置
namespace CasualCastle.Domain.Battle;

public sealed class TrapConfig
{
	public int Damage;
	public float Range;
	public float Cooldown;
	public float PoisonPerSecond;
	public float PoisonDuration;
	public bool IsTeleport;
	public float AttractRange;
	public float FuseSeconds;
	public float PullSpeed;
}

public static class TrapRules
{
	public const string SpikeTrap = "SpikeTrap";
	public const string PoisonTrap = "PoisonTrap";
	public const string FireTrap = "FireTrap";
	public const string TeleportTrap = "TeleportTrap";
	public const string TreasureBox = "TreasureBox";

	public const int BaseMaxTrapCount = 4;
	public const int MaxTrapCountDoubled = 8;

	public static int GetMaxTrapCount(bool doubled)
	{
		return doubled ? MaxTrapCountDoubled : BaseMaxTrapCount;
	}

	public static bool IsTrap(string typeId)
	{
		return typeId == SpikeTrap || typeId == PoisonTrap || typeId == FireTrap
			|| typeId == TeleportTrap || typeId == TreasureBox;
	}

	public static TrapConfig GetConfig(string typeId)
	{
		switch (typeId)
		{
			case SpikeTrap:
				return new TrapConfig { Damage = 25, Range = 75f, Cooldown = 2f };
			case PoisonTrap:
				return new TrapConfig { Damage = 15, Range = 85f, Cooldown = 3f, PoisonPerSecond = 4f, PoisonDuration = 3f };
			case FireTrap:
				return new TrapConfig { Damage = 45, Range = 90f, Cooldown = 3.5f, PoisonPerSecond = 6f, PoisonDuration = 2f };
			case TeleportTrap:
				return new TrapConfig { Range = 75f, Cooldown = 8f, IsTeleport = true };
			case TreasureBox:
				return new TrapConfig { Damage = 60, Range = 110f, Cooldown = 12f, AttractRange = 160f, FuseSeconds = 3f, PullSpeed = 40f };
			default:
				return new TrapConfig();
		}
	}
}