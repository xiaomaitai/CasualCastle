// 阵营资源池：对局/阵营级共享资源接入口，Unit 经此访问而非直接持有单例
namespace CasualCastle.Domain.Battle;

public sealed class FactionResources
{
	public BlackMagicService BlackMagic { get; set; }

	public ManaService Mana { get; set; }

	public ComfortService Comfort { get; set; }

	public FactionResources()
	{
	}

	public FactionResources(BlackMagicService blackMagic, ManaService mana, ComfortService comfort)
	{
		BlackMagic = blackMagic;
		Mana = mana;
		Comfort = comfort;
	}
}
