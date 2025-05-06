namespace SDG.Unturned;

public struct LiveConfigItemCraftingRecipe : IDatParseable
{
    public int targetItemDefId;

    public int craftingMaterialsRequired;

    public bool TryParse(IDatNode node)
    {
        if (node is IDatDictionary dictionary)
        {
            targetItemDefId = dictionary.ParseInt32("ItemDefId");
            craftingMaterialsRequired = dictionary.ParseInt32("Materials");
            if (targetItemDefId > 0)
            {
                return craftingMaterialsRequired > 0;
            }
            return false;
        }
        return false;
    }
}
