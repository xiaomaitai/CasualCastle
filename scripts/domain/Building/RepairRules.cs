namespace CasualCastle.Domain.Building;

public static class RepairRules
{
    public static bool CanRepair(int health, int maxHealth, bool isCore, bool isPlayerOwned,
        bool hasEnemyOnTop, bool isPlaying, bool isNight)
    {
        if (health >= maxHealth)
            return false;
        if (isCore)
            return false;
        if (!isPlayerOwned)
            return false;
        if (hasEnemyOnTop)
            return false;
        if (!isPlaying)
            return false;
        if (!isNight)
            return false;
        return true;
    }

    public static int GetRepairCost(int maxHealth, int health, int goldPerHealth)
    {
        return (maxHealth - health) * goldPerHealth;
    }
}
