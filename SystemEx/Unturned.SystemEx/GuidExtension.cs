using System;

namespace Unturned.SystemEx;

public static class GuidExtension
{
    public static bool IsEmpty(this Guid guid)
    {
        return guid.Equals(Guid.Empty);
    }
}
