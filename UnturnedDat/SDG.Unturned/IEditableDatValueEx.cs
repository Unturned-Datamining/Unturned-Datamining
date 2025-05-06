namespace SDG.Unturned;

public static class IEditableDatValueEx
{
    public static TValueNode SetInlineComment<TValueNode>(this TValueNode valueNode, string inlineComment) where TValueNode : IEditableDatValue
    {
        valueNode.InlineComment = inlineComment;
        return valueNode;
    }
}
