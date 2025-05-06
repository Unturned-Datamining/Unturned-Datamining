using System;
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
    /// If greater than zero, find this zombie type configured in the level editor. For example, if the level editor
    /// lists "0 Fire (4)", then 4 is the unique ID, and if assigned to this condition a zombie from the "Fire"
    /// table will spawn.
    /// </summary>
    public int LevelTableUniqueId { get; private set; }

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
        return Local.FormatText(text, num, value);
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(id);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16("Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseEnum<EZombieSpeciality>("Zombie", out var eZombieSpeciality))
        {
            zombie = eZombieSpeciality;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Zombie");
        }
        spawn = p.data.ParseBool("Spawn");
        spawnQuantity = p.data.ParseInt32("Spawn_Quantity", 1);
        nav = p.data.ParseUInt8("Nav", byte.MaxValue);
        sqrRadius = MathfEx.Square(p.data.ParseFloat("Radius", 512f));
        sqrMinRadius = MathfEx.Square(p.data.ParseFloat("MinRadius"));
        LevelTableUniqueId = p.data.ParseInt32("LevelTableOverride", -1);
        usesBossInterval = spawnQuantity < 2;
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseEnum<EZombieSpeciality>(p.legacyPrefix + "_Zombie", out var eZombieSpeciality))
        {
            zombie = eZombieSpeciality;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Zombie");
        }
        spawn = p.data.ContainsKey(p.legacyPrefix + "_Spawn");
        spawnQuantity = p.data.ParseInt32(p.legacyPrefix + "_Spawn_Quantity", 1);
        nav = p.data.ParseUInt8(p.legacyPrefix + "_Nav", byte.MaxValue);
        sqrRadius = MathfEx.Square(p.data.ParseFloat(p.legacyPrefix + "_Radius", 512f));
        sqrMinRadius = MathfEx.Square(p.data.ParseFloat(p.legacyPrefix + "_MinRadius"));
        LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTableOverride", -1);
        usesBossInterval = spawnQuantity < 2;
    }

    public NPCZombieKillsCondition()
    {
    }

    [Obsolete]
    public NPCZombieKillsCondition(ushort newID, short newValue, EZombieSpeciality newZombie, bool newSpawn, int newSpawnQuantity, byte newNav, float newRadius, float newMinRadius, int newLevelTableUniqueId, string newText, bool newShouldReset)
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
        LevelTableUniqueId = newLevelTableUniqueId;
        usesBossInterval = spawnQuantity < 2;
    }
}
