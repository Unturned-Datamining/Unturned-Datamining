namespace SDG.Unturned;

public static class NPCLogicTypeEx
{
    public static char ToCharAbbr(this ENPCLogicType type)
    {
        return type switch
        {
            ENPCLogicType.LESS_THAN => '<', 
            ENPCLogicType.LESS_THAN_OR_EQUAL_TO => '≤', 
            ENPCLogicType.EQUAL => '=', 
            ENPCLogicType.NOT_EQUAL => '≠', 
            ENPCLogicType.GREATER_THAN_OR_EQUAL_TO => '≥', 
            ENPCLogicType.GREATER_THAN => '>', 
            _ => '-', 
        };
    }
}
