using UnityEngine;

namespace SDG.Unturned;

public class Skill
{
    public byte level;

    public byte max;

    public int maxUnlockableLevel = -1;

    public float costMultiplier = 1f;

    private uint _cost;

    private float difficulty;

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

    public uint cost => MathfEx.RoundAndClampToUInt((float)_cost * ((float)(int)level * difficulty + 1f) * costMultiplier);

    public int GetClampedMaxUnlockableLevel()
    {
        if (maxUnlockableLevel <= -1)
        {
            return max;
        }
        return Mathf.Min(max, maxUnlockableLevel);
    }

    public void setLevelToMax()
    {
        level = max;
    }

    public Skill(byte newLevel, byte newMax, uint newCost, float newDifficulty)
    {
        level = newLevel;
        max = newMax;
        _cost = newCost;
        difficulty = newDifficulty;
    }
}
