using UnityEngine;

namespace SDG.Unturned;

public class NPCRandomShortFlagReward : NPCShortFlagReward
{
    public short minValue { get; protected set; }

    public short maxValue { get; protected set; }

    public override short value
    {
        get
        {
            return (short)Random.Range(minValue, maxValue + 1);
        }
        protected set
        {
        }
    }

    public override void grantReward(Player player, bool shouldSend)
    {
        if (Provider.isServer)
        {
            base.grantReward(player, shouldSend: true);
        }
    }

    public NPCRandomShortFlagReward(ushort newID, short newMinValue, short newMaxValue, ENPCModificationType newModificationType, string newText)
        : base(newID, 0, newModificationType, newText)
    {
        base.id = newID;
        minValue = newMinValue;
        maxValue = newMaxValue;
        base.modificationType = newModificationType;
    }
}
