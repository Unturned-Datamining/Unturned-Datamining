namespace SDG.Unturned;

public class NPCCurrencyReward : INPCReward
{
    public AssetReference<ItemCurrencyAsset> currency { get; protected set; }

    public uint value { get; protected set; }

    public override void GrantReward(Player player)
    {
        currency.Find()?.grantValue(player, value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            ItemCurrencyAsset itemCurrencyAsset = currency.Find();
            if (itemCurrencyAsset != null && !string.IsNullOrEmpty(itemCurrencyAsset.valueFormat))
            {
                text = itemCurrencyAsset.valueFormat;
            }
            else
            {
                text = PlayerNPCQuestUI.localization.read("Reward_Currency");
            }
        }
        return string.Format(text, value);
    }

    public NPCCurrencyReward(AssetReference<ItemCurrencyAsset> newCurrency, uint newValue, string newText)
        : base(newText)
    {
        currency = newCurrency;
        value = newValue;
    }
}
