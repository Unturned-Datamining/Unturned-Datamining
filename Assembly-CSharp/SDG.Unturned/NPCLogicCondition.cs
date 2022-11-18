using System;

namespace SDG.Unturned;

public class NPCLogicCondition : INPCCondition
{
    public ENPCLogicType logicType { get; protected set; }

    protected bool doesLogicPass<T>(T a, T b) where T : IComparable
    {
        return NPCTool.doesLogicPass(logicType, a, b);
    }

    public NPCLogicCondition(ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        logicType = newLogicType;
    }
}
