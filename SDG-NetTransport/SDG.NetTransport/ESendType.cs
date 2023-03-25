using System;

namespace SDG.NetTransport;

[Obsolete]
public enum ESendType
{
    RELIABLE,
    RELIABLE_NODELAY,
    UNRELIABLE,
    UNRELIABLE_NODELAY
}
