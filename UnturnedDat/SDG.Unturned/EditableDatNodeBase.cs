namespace SDG.Unturned;

internal abstract class EditableDatNodeBase
{
    public int creationId;

    public IDatNode parentNode;

    public string comment;

    public bool hasAssignedComment;

    public string Comment
    {
        get
        {
            return comment;
        }
        set
        {
            comment = value;
            hasAssignedComment = true;
        }
    }

    public int TopMargin { get; set; }

    public int BottomMargin { get; set; }
}
