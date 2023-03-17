using System.Collections.Generic;
using SDG.NetTransport;

namespace SDG.Unturned;

public class PooledTransportConnectionList : List<ITransportConnection>
{
    internal PooledTransportConnectionList(int capacity)
        : base(capacity)
    {
    }
}
