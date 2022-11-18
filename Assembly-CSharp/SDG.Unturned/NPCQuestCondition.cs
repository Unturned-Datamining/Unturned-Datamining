namespace SDG.Unturned;

public class NPCQuestCondition : NPCLogicCondition
{
    public ushort id { get; protected set; }

    public ENPCQuestStatus status { get; protected set; }

    public bool ignoreNPC { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.quests.getQuestStatus(id), status);
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (!shouldReset)
        {
            return;
        }
        if (shouldSend)
        {
            UnturnedLog.error("Resetting NPC quest condition over network not supported. ID: {0} Status: {1}");
            return;
        }
        switch (status)
        {
        case ENPCQuestStatus.NONE:
            UnturnedLog.error("Reset none quest status? How should this work?");
            break;
        case ENPCQuestStatus.ACTIVE:
            player.quests.abandonQuest(id);
            break;
        case ENPCQuestStatus.READY:
            player.quests.completeQuest(id, ignoreNPC);
            break;
        case ENPCQuestStatus.COMPLETED:
            player.quests.removeFlag(id);
            break;
        }
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    public NPCQuestCondition(ushort newID, ENPCQuestStatus newStatus, bool newIgnoreNPC, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        id = newID;
        status = newStatus;
        ignoreNPC = newIgnoreNPC;
    }
}
