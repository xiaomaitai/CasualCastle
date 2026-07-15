using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public interface ISkillRepository
{
	SkillDef Get(string skillId);
	IReadOnlyList<SkillDef> GetByUnitType(string unitTypeId);
}
