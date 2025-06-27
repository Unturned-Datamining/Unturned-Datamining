namespace SDG.Unturned;

public interface IOwnershipInfo
{
    bool TryGetOwnership(out ulong ownerUser, out ulong ownerGroup);
}
