using System;

namespace SDG.Unturned;

public interface IAssetReference
{
    /// <summary>
    /// GUID of the asset this is referring to.
    /// </summary>
    Guid GUID { get; set; }

    bool isValid { get; }
}
