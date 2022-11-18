using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SDG.Unturned;

public static class UnturnedAnalytics
{
    [Conditional("WITH_ANALYTICS")]
    public static void CustomEvent(string id, Dictionary<string, object> eventData)
    {
        throw new NotImplementedException();
    }
}
