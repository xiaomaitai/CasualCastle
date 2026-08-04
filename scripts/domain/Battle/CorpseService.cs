// 尸体服务：逻辑尸体留存期满后通过延迟调度从战场仓库移除
namespace CasualCastle.Domain.Battle;

public class CorpseService
{
	private readonly IDelayScheduler _delayScheduler;
	private readonly IFieldUnitRepository _fieldRepo;

	public CorpseService(IDelayScheduler delayScheduler, IFieldUnitRepository fieldRepo)
	{
		_delayScheduler = delayScheduler;
		_fieldRepo = fieldRepo;
	}

	public void ScheduleCorpseRemoval(IUnitHandle corpse)
	{
		_delayScheduler.Schedule(CorpseRules.RetentionSeconds, () => _fieldRepo.Unregister(corpse));
	}
}
