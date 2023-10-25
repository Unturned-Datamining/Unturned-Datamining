using System;
using System.Diagnostics;

namespace SDG.Unturned;

/// <summary>
/// Report success or failure from game systems, conditionally compiled into the Windows 64-bit build.
/// </summary>
public class ContinuousIntegration
{
    /// <summary>
    /// Call when the server is done all loading without running into errors.
    /// Ignored if not running in CI mode, otherwise exits the server successfully with error code 0.
    /// </summary>
    [Conditional("DEVELOPMENT_BUILD")]
    public static void reportSuccess()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Call when the server encounters any error.
    /// Ignored if not running in CI mode, otherwise exits the server with error code 1.
    /// </summary>
    [Conditional("DEVELOPMENT_BUILD")]
    public static void reportFailure(object message)
    {
        throw new NotImplementedException();
    }
}
