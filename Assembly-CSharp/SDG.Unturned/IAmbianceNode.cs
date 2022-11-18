using System;

namespace SDG.Unturned;

public interface IAmbianceNode
{
    [Obsolete]
    ushort id { get; }

    bool noWater { get; }

    bool noLighting { get; }

    EffectAsset GetEffectAsset();
}
