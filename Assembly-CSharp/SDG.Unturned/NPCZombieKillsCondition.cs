using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCZombieKillsCondition : INPCCondition
{
    /// <summary>
    /// Only kills outside this radius around the player are tracked.
    /// NSTM requested this for a sniping zombies quest.
    /// </summary>
    public float sqrMinRadius;

    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public EZombieSpeciality zombie { get; protected set; }

    /// <summary>
    /// Should zombie(s) of the required type be spawned when player enters the area?
    /// </summary>
    public bool spawn { get; protected set; }

    /// <summary>
    /// How many to spawn if spawning <see cref="P:SDG.Unturned.NPCZombieKillsCondition.spawn" /> is enabled.
    /// </summary>
    public int spawnQuantity { get; protected set; }

    /// <summary>
    /// Navmesh index player must be within. If set to byte.MaxValue then anywhere on the map is eligible.
    /// </summary>
    public byte nav { get; protected set; }

    /// <summary>
    /// Only kills within this radius around the player are tracked.
    /// </summary>
    public float sqrRadius { get; protected set; }

    /// <summary>
    /// If spawning is enabled, whether to use the timer between spawns.
    /// </summary>
    public bool usesBossInterval { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(id, out var num))
        {
            return num >= value;
        }
        return false;
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.quests.sendRemoveFlag(id);
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
