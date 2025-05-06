using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public struct ScopedPlayerInventorySearchResultPool : IDisposable
{
    private bool hasClaimed;

    private List<PlayerInventorySearchResultV2> _results;

    public List<PlayerInventorySearchResultV2> PooledResults
    {
        get
        {
            if (!hasClaimed)
            {
                hasClaimed = true;
                _results = PlayerInventorySearchResultPool.Claim();
            }
            return _results;
        }
    }

    public void Dispose()
    {
        if (_results != null)
        {
            PlayerInventorySearchResultPool.Release(PooledResults);
            _results = null;
        }
    }
}
