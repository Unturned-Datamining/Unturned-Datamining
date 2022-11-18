namespace SDG.Unturned;

public static class SlotTypeExtension
{
    public static bool canEquipAsPrimary(this ESlotType slotType)
    {
        if (slotType != ESlotType.PRIMARY && slotType != ESlotType.SECONDARY)
        {
            return slotType == ESlotType.ANY;
        }
        return true;
    }

    public static bool canEquipAsSecondary(this ESlotType slotType)
    {
        if (slotType != ESlotType.SECONDARY)
        {
            return slotType == ESlotType.ANY;
        }
        return true;
    }

    public static bool canEquipFromBag(this ESlotType slotType)
    {
        if (slotType != ESlotType.PRIMARY)
        {
            return slotType != ESlotType.SECONDARY;
        }
        return false;
    }

    public static bool canEquipInPage(this ESlotType slotType, byte page)
    {
        return page switch
        {
            0 => slotType.canEquipAsPrimary(), 
            1 => slotType.canEquipAsSecondary(), 
            _ => slotType.canEquipFromBag(), 
        };
    }
}
