using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by name A to Z.
/// </summary>
internal class ServerBookmarkComparer_NameAscending : IComparer<ServerBookmarkDetails>
{
    public virtual int Compare(ServerBookmarkDetails lhs, ServerBookmarkDetails rhs)
    {
        return lhs.name.CompareTo(rhs.name);
    }
}
