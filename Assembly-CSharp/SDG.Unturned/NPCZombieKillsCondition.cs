using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCZombieKillsCondition : INPCCondition
{
    public float sqrMinRadius;

    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public EZombieSpeciality zombie { get; protected set; }

    public bool spawn { get; protected set; }

    public int spawnQuantity { get; protected set; }

    public byte nav { get; protected set; }

    public float sqrRadius { get; protected set; }

    public bool usesBossInterval { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(id, out var num))
        {
            return num >= value;
        }
        return false;
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (shouldReset)
        {
            if (shouldSend)
            {
                player.quests.sendRemoveFlag(id);
            }
            else
            {
                player.quests.removeFlag(id);
            }
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.format("Condition_ZombieKills");
        }
        if (!player.quests.getFlag(id, out var num))
        {
            num = 0;
        }
        return string.Format(text, num, value);
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(id);
    }

    public NPCZombieKillsCondition(ushort newID, short newValue, EZombieSpeciality newZombie, bool newSpawn, int newSpawnQuantity, byte newNav, float newRadius, float newMinRadius, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        id = newID;
        value = newValue;
        zombie = newZombie;
        spawn = newSpawn;
        spawnQuantity = newSpawnQuantity;
        nav = newNav;
        sqrRadius = MathfEx.Square(newRadius);
        sqrMinRadius = MathfEx.Square(newMinRadius);
        usesBossInterval = spawnQuantity < 2;
    }
}
