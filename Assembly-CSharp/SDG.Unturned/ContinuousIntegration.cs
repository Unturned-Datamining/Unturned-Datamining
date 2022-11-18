using System;
using System.Diagnostics;

namespace SDG.Unturned;

public class ContinuousIntegration
{
    [Conditional("DEVELOPMENT_BUILD")]
    public static void reportSuccess()
    {
        throw new NotImplementedException();
    }

    [Conditional("DEVELOPMENT_BUILD")]
    public static void reportFailure(object message)
    {
        throw new NotImplementedException();
    }
}
