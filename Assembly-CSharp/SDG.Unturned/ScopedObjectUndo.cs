using System;
using SDG.Framework.Devkit.Transactions;

namespace SDG.Unturned;

public class ScopedObjectUndo : IDisposable
{
    public ScopedObjectUndo(object modifiedObject)
    {
        DevkitTransactionUtility.beginGenericTransaction();
        DevkitTransactionUtility.recordObjectDelta(modifiedObject);
    }

    public void Dispose()
    {
        DevkitTransactionUtility.endGenericTransaction();
    }
}
