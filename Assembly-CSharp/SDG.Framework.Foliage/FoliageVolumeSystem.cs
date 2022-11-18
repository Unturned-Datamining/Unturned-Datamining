using System;
using System.Collections.Generic;
using SDG.Unturned;

namespace SDG.Framework.Foliage;

public static class FoliageVolumeSystem
{
    [Obsolete]
    public static List<FoliageVolume> additiveVolumes => VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().additiveVolumes;

    [Obsolete]
    public static List<FoliageVolume> subtractiveVolumes => VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().subtractiveVolumes;
}
