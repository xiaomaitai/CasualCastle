public static class UnitDisplayName
{
	public static string Get(string typeId)
	{
		return typeId switch
		{
			"Spearman" => "枪兵",
			"ShieldBearer" => "盾兵",
			"Archer" => "弓箭手",
			"Knight" => "骑士",
			"Scout" => "斥候",
			"Swordsman" => "剑士",
			"HeavyShield" => "重盾兵",
			"Crossbowman" => "弩手",
			"HeavyCavalry" => "重骑兵",
			"LightCavalry" => "轻骑兵",
			"HolyShieldGuardian" => "圣盾守护者",
			"DragonRiderCommander" => "龙骑统帅",
			"ShadowLord" => "暗影之主",
			"King" => "国王",
			"RoyalGuard" => "皇家禁卫",
			"RoyalShieldGuard" => "皇家盾卫",
			"RoyalSharpshooter" => "皇家神射手",
			"RoyalKnight" => "皇家骑士",
			"RoyalScout" => "皇家斥候",
			"ElfRanger" => "精灵游侠",
			"ElfSharpshooter" => "精灵神射手",
			"DwarfWarrior" => "矮人战士",
			"DwarfBerserker" => "矮人狂战士",
			"HalflingThief" => "半身人盗贼",
			"HalflingAssassin" => "半身人刺客",
			"DragonbornMage" => "龙裔术士",
			"DragonbornArchmage" => "龙裔大法师",
			"HolyShieldBearer" => "圣盾兵",
			"Paladin" => "圣骑士",
			"Priest" => "牧师",
			"Archbishop" => "大主教",
			"Inquisitor" => "审判官",
			"Punisher" => "惩戒者",
			_ => typeId,
		};
	}
}
