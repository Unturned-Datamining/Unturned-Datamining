namespace SDG.Unturned;

public class IgnoredCraftingBlueprint
{
    public ushort itemId;

    public byte blueprintIndex;

    public bool matchesBlueprint(Blueprint blueprint)
    {
        if (itemId != blueprint.sourceItem.id)
        {
            return false;
        }
        return blueprintIndex == blueprint.id;
    }
}
