namespace SDG.Unturned;

public static class IEditableDatNodeEx
{
    public static TNode SetComment<TNode>(this TNode node, string comment) where TNode : IEditableDatNode
    {
        node.Comment = comment;
        return node;
    }

    public static TNode SetMargins<TNode>(this TNode node, int margins) where TNode : IEditableDatNode
    {
        node.TopMargin = margins;
        node.BottomMargin = margins;
        return node;
    }

    public static TNode SetMargins<TNode>(this TNode node, int topMargin, int bottomMargin) where TNode : IEditableDatNode
    {
        node.TopMargin = topMargin;
        node.BottomMargin = bottomMargin;
        return node;
    }

    public static TNode SetTopMargin<TNode>(this TNode node, int topMargin) where TNode : IEditableDatNode
    {
        node.TopMargin = topMargin;
        return node;
    }

    public static TNode SetBottomMargin<TNode>(this TNode node, int bottomMargin) where TNode : IEditableDatNode
    {
        node.BottomMargin = bottomMargin;
        return node;
    }
}
