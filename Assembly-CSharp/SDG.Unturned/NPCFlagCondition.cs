namespace SDG.Unturned;

public class NPCFlagCondition : NPCLogicCondition
{
    public ushort id { get; protected set; }

    public bool allowUnset { get; protected set; }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    public NPCFlagCondition(ushort newID, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        id = newID;
        allowUnset = newAllowUnset;
    }
}
