namespace SDG.Unturned;

/// <summary>
/// Sort servers by name Z to A.
/// </summary>
internal class ServerBookmarkComparer_NameDescending : ServerBookmarkComparer_NameAscending
{
    public override int Compare(ServerBookmarkDetails lhs, ServerBookmarkDetails rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
