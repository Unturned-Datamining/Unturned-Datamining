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

    internal PlayerQuest(QuestAsset asset)
    {
        this.asset = asset;
        id = asset?.id ?? 0;
    }
}
