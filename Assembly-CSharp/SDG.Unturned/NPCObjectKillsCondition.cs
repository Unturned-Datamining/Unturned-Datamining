using System;

namespace SDG.Unturned;

public class NPCObjectKillsCondition : INPCCondition
{
    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public Guid objectGuid { get; protected set; }

    public byte nav { get; protected set; }

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
            text = PlayerNPCQuestUI.localization.format("Condition_ObjectKills");
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

    public NPCObjectKillsCondition(ushort newID, short newValue, Guid newObjectGuid, byte newNav, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        id = newID;
        value = newValue;
        objectGuid = newObjectGuid;
        nav = newNav;
    }
}
