namespace SDG.Unturned;

public class NPCCurrencyCondition : NPCLogicCondition
{
    public AssetReference<ItemCurrencyAsset> currency { get; protected set; }

    public uint value { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        ItemCurrencyAsset itemCurrencyAsset = currency.Find();
        if (itemCurrencyAsset == null)
        {
            return false;
        }
        uint inventoryValue = itemCurrencyAsset.getInventoryValue(player);
        return doesLogicPass(inventoryValue, value);
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            currency.Find()?.spendValue(player, value);
        }
    }

    public override string formatCondition(Player player)
    {
        ItemCurrencyAsset itemCurrencyAsset = currency.Find();
        if (itemCurrencyAsset == null)
        {
            return "?";
        }
        if (string.IsNullOrEmpty(text))
        {
            if (!string.IsNullOrEmpty(itemCurrencyAsset.defaultConditionFormat))
            {
                text = itemCurrencyAsset.defaultConditionFormat;
            }
            else
            {
                text = PlayerNPCQuestUI.localization.format("Condition_Currency");
            }
        }
        uint inventoryValue = itemCurrencyAsset.getInventoryValue(player);
        return string.Format(text, inventoryValue, value);
    }

    public NPCCurrencyCondition(AssetReference<ItemCurrencyAsset> newCurrency, uint newValue, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        currency = newCurrency;
        value = newValue;
    }
}
