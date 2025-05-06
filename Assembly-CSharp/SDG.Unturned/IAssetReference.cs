using System;

namespace SDG.Unturned;

[Obsolete("This interface was essentially pointless/unused.")]
public interface IAssetReference
{
    /// <summary>
    /// GUID of the asset this is referring to.
    /// </summary>
    Guid GUID { get; set; }

    bool isValid { get; }
}
