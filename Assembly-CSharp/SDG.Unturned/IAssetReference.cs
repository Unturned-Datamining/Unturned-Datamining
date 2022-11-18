using System;

namespace SDG.Unturned;

public interface IAssetReference
{
    Guid GUID { get; set; }

    bool isValid { get; }
}
