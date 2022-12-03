namespace SDG.Unturned;

public class PlayerQuest
{
    public ushort id { get; private set; }

    public QuestAsset asset { get; protected set; }

    public PlayerQuest(ushort newID)
    {
        id = newID;
        asset = Assets.find(EAssetType.NPC, id) as QuestAsset;
    }
}
