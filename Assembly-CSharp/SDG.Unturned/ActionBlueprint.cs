namespace SDG.Unturned;

public class ActionBlueprint
{
    private byte _id;

    private bool _isLink;

    public byte id => _id;

    public bool isLink => _isLink;

    public ActionBlueprint(byte newID, bool newLink)
    {
        _id = newID;
        _isLink = newLink;
    }
}
