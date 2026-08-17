// 视觉资源仓库端口：按类型标识查询技能图标、单位肖像与建筑贴图的资源 uid
namespace CasualCastle.Domain.Shared;

public interface IVisualAssetRepository
{
	string GetSkillIconUid(string skillId);
	string GetPortraitUid(string typeId);
	string GetBuildingTextureUid(string buildingTypeId);
}
