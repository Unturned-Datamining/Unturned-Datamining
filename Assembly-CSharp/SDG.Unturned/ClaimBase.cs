namespace SDG.Unturned;

public class ClaimBase
{
    public ulong owner;

    public ulong group;

    public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

    public ClaimBase(ulong newOwner, ulong newGroup)
    {
        owner = newOwner;
        group = newGroup;
    }
}
