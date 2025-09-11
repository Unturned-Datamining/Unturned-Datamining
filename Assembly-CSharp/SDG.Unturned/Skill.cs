using UnityEngine;

namespace SDG.Unturned;

public class Skill
{
    public byte level;

    /// <summary>
    /// Vanilla maximum level.
    /// </summary>
    public byte max;

    /// <summary>
    /// If set, maximum skill level attainable through gameplay.
    /// </summary>
    public int maxUnlockableLevel = -1;

    /// <summary>
    /// Multiplier for XP upgrade cost.
    /// </summary>
    public float costMultiplier = 1f;

    internal int baseCost;

    internal int perLevelCostIncrease;

    public float mastery
    {
        get
        {
            if (level == 0)
            {
                return 0f;
            }
            if (level >= max)
            {
                return 1f;
            }
            return (float)(int)level / (float)(int)max;
        }
    }

    public uint cost => MathfEx.RoundAndClampToUInt((float)(baseCost + level * perLevelCostIncrease) * costMultiplier);

    /// <summary>
    /// Get maximum level, or maxUnlockableLevel if set.
    /// </summary>
    /// <returns></returns>
    public int GetClampedMaxUnlockableLevel()
    {
        if (maxUnlockableLevel <= -1)
        {
            return max;
        }
        return Mathf.Min(max, maxUnlockableLevel);
    }

    public float NormalizeLevel(int inputLevel)
    {
        if (inputLevel <= 0)
        {
            return 0f;
        }
        if (inputLevel >= max)
        {
            return 1f;
        }
        return (float)inputLevel / (float)(int)max;
    }

    public void setLevelToMax()
    {
        level = max;
    }

    public Skill(byte newLevel, byte newMax, uint newCost, float newDifficulty)
    {
        level = newLevel;
        max = newMax;
        baseCost = (int)newCost;
        perLevelCostIncrease = Mathf.RoundToInt((float)baseCost * newDifficulty);
    }
}
