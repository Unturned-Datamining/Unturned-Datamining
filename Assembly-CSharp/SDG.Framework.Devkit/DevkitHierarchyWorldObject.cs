using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;

namespace SDG.Framework.Devkit;

[Obsolete]
public class DevkitHierarchyWorldObject : DevkitHierarchyWorldItem
{
    public AssetReference<MaterialPaletteAsset> customMaterialOverride;

    public int materialIndexOverride = -1;

    public Guid GUID;

    public ELevelObjectPlacementOrigin placementOrigin;

    public override bool ShouldSave => false;

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        GUID = reader.readValue<Guid>("GUID");
        placementOrigin = reader.readValue<ELevelObjectPlacementOrigin>("Origin");
        customMaterialOverride = reader.readValue<AssetReference<MaterialPaletteAsset>>("Custom_Material_Override");
        if (reader.containsKey("Material_Index_Override"))
        {
            materialIndexOverride = reader.readValue<int>("Material_Index_Override");
        }
        else
        {
            materialIndexOverride = -1;
        }
        LevelHierarchy.instance.loadedAnyDevkitObjects = true;
    }

    protected void OnEnable()
    {
        LevelHierarchy.addItem(this);
    }

    protected void OnDisable()
    {
        LevelHierarchy.removeItem(this);
    }

    protected void Start()
    {
        NetId devkitObjectNetId = LevelNetIdRegistry.GetDevkitObjectNetId(instanceID);
        LevelObjects.registerDevkitObject(new LevelObject(base.inspectablePosition, base.inspectableRotation, base.inspectableScale, 0, GUID, placementOrigin, instanceID, customMaterialOverride, materialIndexOverride, devkitObjectNetId), out var _, out var _);
    }
}
